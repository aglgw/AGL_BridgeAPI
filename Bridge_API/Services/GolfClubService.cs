using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using System.Diagnostics;
using AGL.Api.ApplicationCore.Utilities;
using StackExchange.Redis;

namespace AGL.Api.Bridge_API.Services
{
    public class GolfClubService : IGolfClubService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        private readonly IRedisService _redisService;

        public GolfClubService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService, IRedisService redisService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
            _redisService = redisService;
        }


        public async Task<IDataResult> PostGolfClub(GolfClubInfo request, string supplierCode)
        {
            return await ProcessGolfClub(request, supplierCode, request.golfClubCode, "POST");
        }

        public async Task<IDataResult> PutGolfClub(GolfClubInfo request, string supplierCode)
        {
            return await ProcessGolfClub(request, supplierCode, request.golfClubCode, "PUT");
        }

        public async Task<OAPIDataResponse<List<GolfClubInfo>>> GetGolfClub(string supplierCode, string golfClubCode)
        {
            try
            {
                // 공급사 코드로 공급사 ID 조회
                OAPI_Supplier supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                int supplierId = supplier.SupplierId;

                // 모든 관련 데이터를 미리 조회 골프장,이미지,환불정책,코스,홀
                var existingGolfclubQuery = _context.GolfClubs
                    .Include(g => g.GolfClubImages)
                    .Include(g => g.RefundPolicies)
                    .Include(g => g.Courses)
                    .Include(g => g.Holes)
                    .Where(g => g.Supplier != null && g.SupplierId == supplierId);

                // 골프장 코드가 있을 경우 해당 코드들에 대한 조건 추가
                if (golfClubCode != null)
                {
                    existingGolfclubQuery = existingGolfclubQuery.Where(g => g.GolfClubCode == golfClubCode);
                }
                // 조건에 맞는 TeeTimePriceMappings 목록을 조회
                var existingGolfclubs = await existingGolfclubQuery.ToListAsync();

                // 유효성 검사 - 조회된 골프장이 없을 경우
                if (existingGolfclubs == null || !existingGolfclubs.Any())
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "GolfClubList", "골프장 검색 코드 없음");
                    return await _commonService.CreateResponse<List<GolfClubInfo>>(false, ResultCode.NOT_FOUND, "GolfClubs Not Found", null);
                }

                var golfClubDtos = existingGolfclubs.Select(golfClub => new GolfClubInfo
                {
                    golfClubCode = golfClub.GolfClubCode,
                    golfClubName = golfClub.GolfClubName,
                    countryCode = golfClub.CountryCode,
                    language = golfClub.Language,
                    currency = golfClub.Currency,
                    description = golfClub.Description,
                    address = golfClub.Address,
                    latitude = golfClub.Latitude?.ToString() ?? "0",
                    longitude = golfClub.Longitude?.ToString() ?? "0",
                    phone = golfClub.Phone,
                    fax = golfClub.Fax,
                    email = golfClub.Email,
                    homepage = golfClub.Homepage,
                    totalHoleCount = golfClub.TotalHoleCount ?? 0,
                    totalCourseCount = golfClub.Courses?.Count ?? 0,
                    isGuestInfoRequired = golfClub.isGuestInfoRequired,
                    image = golfClub.GolfClubImages?.Select(img => new Images
                    {
                        id = img.Idx,
                        url = img.Url,
                        title = img.Title,
                        description = img.ImageDescription
                    }).ToList() ?? new List<Images>(),
                    refundPolicy = golfClub.RefundPolicies?.Select(rp => new RefundPolicy
                    {
                        refundDate = rp.RefundDate,
                        refundFee = rp.RefundFee,
                        refundUnit = rp.RefundUnit
                    }).ToList() ?? new List<RefundPolicy>(),
                    course = golfClub.Courses?.Select(c => new Course
                    {
                        courseCode = c.CourseCode,
                        courseName = c.CourseName,
                        courseHoleCount = c.CourseHoleCount,
                        startHole = c.StartHole
                    }).ToList() ?? new List<Course>(),
                    holeInfo = golfClub.Holes?.Select(h => new HoleInfo
                    {
                        holeNumber = h.HoleNumber,
                        holeName = h.HoleName,
                        par = h.Par,
                        distanceUnit = h.DistanceUnit,
                        distance = h.Distance
                    }).ToList() ?? new List<HoleInfo>()
                }).ToList();

                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "GolfClubList", $"골프장 검색 성공");
                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "GolfClub List successfully", golfClubDtos);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "GolfClubList", $"골프장 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<List<GolfClubInfo>>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }


        private async Task<IDataResult> ProcessGolfClub(GolfClubInfo request, string supplierCode, string golfClubCode, string method)
        {
            var golfClub = await _context.GolfClubs.Where(g => g.GolfClubCode == golfClubCode).FirstOrDefaultAsync();

            bool isPost = method.Equals("POST", StringComparison.OrdinalIgnoreCase);

            if (isPost && golfClub != null) // POST: 골프장이 이미 존재하면 오류
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 중복됨", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Golf club already registered", null);
            }
            else if (!isPost && golfClub == null) // PUT or other: 골프장이 존재하지 않으면 오류
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 없음", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Golf club not found", null);
            }

            //var RedisStrKey = $"PGC:" + ComputeSha256.ComputeSha256RequestHash(request);

            //try
            //{
            //    if (await _redisService.KeyExistsAsync(RedisStrKey)) // Redis 키 조회 (비동기)
            //    {
            //        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "GolfClub", $"골프장 중복");
            //        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate request", null);
            //    }
            //    else
            //    {
            //        await _redisService.SetValueAsync(RedisStrKey, "", TimeSpan.FromMinutes(1)); // 비동기로 Redis 키 설정
            //    }
            //}
            //catch (RedisException ex)
            //{
            //    Utils.UtilLogs.LogRegDay(supplierCode, golfClubCode, "GolfClub", $"골프장 Redis 실패 {ex.Message}", true);
            //    return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            //}

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 공급사 코드로 공급사 ID 조회
                        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                        int supplierId = supplier.SupplierId;

                        // 모든 관련 데이터를 미리 조회 골프장,이미지,환불정책,코스,홀
                        var existingGolfclub = await _context.GolfClubs
                            .Include(g => g.GolfClubImages)
                            .Include(g => g.RefundPolicies)
                            .Include(g => g.Courses)
                            .Include(g => g.Holes)
                            .FirstOrDefaultAsync(g => g.Supplier != null && g.SupplierId == supplierId && g.GolfClubCode == golfClubCode);

                        List<OAPI_GolfClubImage> existingImages = existingGolfclub?.GolfClubImages.ToList() ?? new List<OAPI_GolfClubImage>();
                        List<OAPI_GolfClubRefundPolicy> existingRefundPolicies = existingGolfclub?.RefundPolicies.ToList() ?? new List<OAPI_GolfClubRefundPolicy>();
                        List<OAPI_GolfClubCourse> existingCourses = existingGolfclub?.Courses.ToList() ?? new List<OAPI_GolfClubCourse>();
                        List<OAPI_GolfClubHole> existingHoles = existingGolfclub?.Holes.ToList() ?? new List<OAPI_GolfClubHole>();

                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", existingGolfclub != null ? "기존 골프장 정보 조회 성공" : "기존 골프장 정보 없음, 새로 생성 예정");

                        // 골프장 정보
                        int golfClubId;
                        if (existingGolfclub != null)
                        {
                            golfClubId = existingGolfclub.GolfClubId;
                            // 기존 골프장 정보 업데이트
                            existingGolfclub.GolfClubName = request.golfClubName;
                            existingGolfclub.InboundCode = supplier.SupplierCode + "_" + request.golfClubCode;
                            existingGolfclub.CountryCode = request.countryCode;
                            existingGolfclub.Language = request.language;
                            existingGolfclub.Currency = request.currency;
                            existingGolfclub.Description = request.description;
                            existingGolfclub.Address = request.address;
                            existingGolfclub.Latitude = request.latitude;
                            existingGolfclub.Longitude = request.longitude;
                            existingGolfclub.Phone = request.phone;
                            existingGolfclub.Fax = request.fax;
                            existingGolfclub.Email = request.email;
                            existingGolfclub.Homepage = request.homepage;
                            existingGolfclub.TotalHoleCount = request.totalHoleCount;
                            existingGolfclub.TotalCourseCount = request.totalCourseCount;
                            existingGolfclub.isGuestInfoRequired = request.isGuestInfoRequired;
                            existingGolfclub.UpdatedDate = DateTime.UtcNow;
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "기존 골프장 정보 업데이트 시작");
                        }
                        else
                        {
                            var newGolfClub = new OAPI_GolfClub
                            {
                                SupplierId = supplierId,
                                GolfClubCode = request.golfClubCode,
                                InboundCode = supplier.SupplierCode + "_" + request.golfClubCode,
                                GolfClubName = request.golfClubName,
                                CountryCode = request.countryCode,
                                Language = request.language,
                                Currency = request.currency,
                                Description = request.description,
                                Address = request.address,
                                Latitude = request.latitude,
                                Longitude = request.longitude,
                                Phone = request.phone,
                                Fax = request.fax,
                                Email = request.email,
                                Homepage = request.homepage,
                                TotalHoleCount = request.totalHoleCount,
                                TotalCourseCount = request.totalCourseCount,
                                isGuestInfoRequired = request.isGuestInfoRequired,
                                CreatedDate = DateTime.UtcNow
                            };
                            _context.GolfClubs.Add(newGolfClub);
                            await _context.SaveChangesAsync();
                            golfClubId = newGolfClub.GolfClubId;
                            existingGolfclub = newGolfClub;
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "새 골프장 정보 생성 시작");
                        }

                        // 벌크 저장을 위한 리스트 준비
                        var newImages = new List<OAPI_GolfClubImage>();
                        var newRefundPolicies = new List<OAPI_GolfClubRefundPolicy>();
                        var newCourses = new List<OAPI_GolfClubCourse>();
                        var newHoles = new List<OAPI_GolfClubHole>();

                        // 이미지 정보 저장 (유효성 체크)
                        if (request.image != null)
                        {
                            // 요청된 이미지 ID 목록
                            var requestedImageIds = request.image.Select(image => image.id).ToList();

                            foreach (var image in request.image)
                            {
                                var existingImage = existingImages.FirstOrDefault(i => i.Idx == image.id);
                                if (existingImage != null)
                                {
                                    existingImage.Url = image.url;
                                    existingImage.Title = image.title;
                                    existingImage.ImageDescription = image.description;
                                    existingImage.UpdatedDate = DateTime.UtcNow;
                                }
                                else
                                {
                                    var newImage = new OAPI_GolfClubImage
                                    {
                                        GolfClubId = golfClubId,
                                        Idx = image.id,
                                        Url = image.url,
                                        Title = image.title,
                                        ImageDescription = image.description,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    newImages.Add(newImage);
                                }
                            }

                            // 요청 데이터에 없는 기존 이미지를 소프트 삭제
                            var imagesToSoftDelete = existingImages.Where(i => !requestedImageIds.Contains(i.Idx)).ToList();
                            if (imagesToSoftDelete.Any())
                            {
                                foreach (var imageToSoftDelete in imagesToSoftDelete)
                                {
                                    imageToSoftDelete.IsDeleted = true; // 소프트 삭제 처리
                                    imageToSoftDelete.UpdatedDate = DateTime.UtcNow; // 삭제된 시점 기록
                                }
                            }
                        }
                        else
                        {
                            if (existingImages.Any()) // image 가 null 일 때 모든 기존 이미지를 소프트 삭제
                            {
                                foreach (var existingImage in existingImages)
                                {
                                    existingImage.IsDeleted = true; // 소프트 삭제 처리
                                    existingImage.UpdatedDate = DateTime.UtcNow; // 삭제된 시점 기록
                                }
                            }
                        }

                        // 환불 정책 정보 저장 (유효성 체크)
                        if (request.refundPolicy != null)
                        {
                            // 요청된 환불 정책의 RefundDate 목록 생성
                            var requestedRefundDates = request.refundPolicy.Select(rp => rp.refundDate).ToList();

                            foreach (var refundPolicy in request.refundPolicy)
                            {
                                var existingPolicy = existingRefundPolicies.FirstOrDefault(rp => rp.RefundDate == refundPolicy.refundDate);
                                if (existingPolicy != null)
                                {
                                    //existingPolicy.RefundHour = refundPolicy.RefundHour;
                                    existingPolicy.RefundFee = refundPolicy.refundFee;
                                    existingPolicy.RefundHour = refundPolicy.refundHour;
                                    existingPolicy.RefundUnit = refundPolicy.refundUnit;
                                    existingPolicy.UpdatedDate = DateTime.UtcNow;
                                }
                                else
                                {
                                    var newRefundPolicy = new OAPI_GolfClubRefundPolicy
                                    {
                                        GolfClubId = golfClubId,
                                        RefundDate = refundPolicy.refundDate,
                                        RefundHour = refundPolicy.refundHour,
                                        RefundFee = refundPolicy.refundFee,
                                        RefundUnit = refundPolicy.refundUnit,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    newRefundPolicies.Add(newRefundPolicy);
                                }
                            }

                            // 요청 데이터에 없는 기존 환불 정책 소프트 삭제
                            var policiesToSoftDelete = existingRefundPolicies.Where(rp => !requestedRefundDates.Contains(rp.RefundDate)).ToList();
                            if (policiesToSoftDelete.Any())
                            {
                                foreach (var policyToSoftDelete in policiesToSoftDelete)
                                {
                                    policyToSoftDelete.IsDeleted = true; // 소프트 삭제
                                    policyToSoftDelete.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }
                        else
                        {
                            if (existingRefundPolicies.Any()) // refundPolicy 가 null 일때 모든 정책 소프트 삭제
                            {
                                foreach (var existingRefundPolicy in existingRefundPolicies)
                                {
                                    existingRefundPolicy.IsDeleted = true; // 소프트 삭제
                                    existingRefundPolicy.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }

                        // 코스 정보 저장 (유효성 체크)
                        if (request.course != null)
                        {
                            // 요청된 환불 정책의 CoursesCode 목록 생성
                            var requestedCourses = request.course.Select(rp => rp.courseCode).ToList();

                            foreach (var course in request.course)
                            {
                                var existingCourse = existingCourses.FirstOrDefault(c => c.CourseCode.ToString() == course.courseCode.ToString());
                                if (existingCourse != null)
                                {
                                    existingCourse.CourseName = course.courseName;
                                    existingCourse.CourseHoleCount = course.courseHoleCount;
                                    existingCourse.StartHole = course.startHole;
                                    existingCourse.UpdatedDate = DateTime.UtcNow;
                                }
                                else
                                {
                                    var newCourse = new OAPI_GolfClubCourse
                                    {
                                        GolfClubId = golfClubId,
                                        CourseCode = course.courseCode,
                                        CourseName = course.courseName,
                                        CourseHoleCount = course.courseHoleCount,
                                        StartHole = course.startHole,
                                        CreatedDate = DateTime.UtcNow,
                                    };
                                    newCourses.Add(newCourse);
                                }
                            }

                            // 요청 데이터에 없는 기존 코스 소프트 삭제
                            var coursesToSoftDelete = existingCourses.Where(rc => !requestedCourses.Contains(rc.CourseCode)).ToList();
                            if (coursesToSoftDelete.Any())
                            {
                                foreach (var courseToSoftDelete in coursesToSoftDelete)
                                {
                                    courseToSoftDelete.IsDeleted = true; // 소프트 삭제
                                    courseToSoftDelete.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }
                        else
                        {
                            if (existingCourses.Any()) // course 가 null 일때 삭제 처리
                            {
                                foreach (var existingCourse in existingCourses)
                                {
                                    existingCourse.IsDeleted = true; // 소프트 삭제
                                    existingCourse.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }

                        // 홀 정보 저장 (유효성 체크)
                        if (request.holeInfo != null)
                        {
                            // 요청된 환불 정책의 CoursesCode 목록 생성
                            var requestedholeInfos = request.holeInfo.Select(rp => rp.holeNumber).ToList();

                            foreach (var hole in request.holeInfo)
                            {
                                if (hole.holeNumber <= 0)
                                {
                                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "HoleNumber is invalid", null);
                                }

                                var existingHole = existingHoles.FirstOrDefault(h => h.HoleNumber.ToString() == hole.holeNumber.ToString());
                                if (existingHole != null)
                                {
                                    existingHole.Par = hole.par;
                                    existingHole.DistanceUnit = hole.distanceUnit;
                                    existingHole.Distance = hole.distance;
                                    existingHole.UpdatedDate = DateTime.UtcNow;
                                }
                                else
                                {
                                    var newHole = new OAPI_GolfClubHole
                                    {
                                        GolfClubId = golfClubId,
                                        HoleNumber = hole.holeNumber,
                                        Par = hole.par,
                                        DistanceUnit = hole.distanceUnit,
                                        Distance = hole.distance,
                                        CreatedDate = DateTime.UtcNow,
                                        UpdatedDate = null
                                    };
                                    newHoles.Add(newHole);
                                }
                            }

                            // 요청 데이터에 없는 기존 홀 소프트 삭제
                            var holesToSoftDelete = existingHoles.Where(rc => !requestedholeInfos.Contains(rc.HoleNumber)).ToList();
                            if (holesToSoftDelete.Any())
                            {
                                foreach (var holeToSoftDelete in holesToSoftDelete)
                                {
                                    holeToSoftDelete.IsDeleted = true; // 소프트 삭제
                                    holeToSoftDelete.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }
                        else
                        {
                            if (existingHoles.Any()) // holeInfo 가 null 일때 모든 홀 소프트 삭제
                            {
                                foreach (var existingHole in existingHoles)
                                {
                                    existingHole.IsDeleted = true; // 소프트 삭제
                                    existingHole.UpdatedDate = DateTime.UtcNow;
                                }
                            }
                        }

                        // 벌크로 새로운 항목 추가
                        if (newImages.Any())
                        {
                            _context.GolfClubImages.AddRange(newImages);
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 이미지 저장");
                        }
                        if (newRefundPolicies.Any())
                        {
                            _context.GolfClubRefundPolicies.AddRange(newRefundPolicies);
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 환불정책 저장");
                        }
                        if (newCourses.Any())
                        {
                            _context.GolfClubCourses.AddRange(newCourses);
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 코스 저장");
                        }
                        if (newHoles.Any())
                        {
                            _context.GolfClubHoles.AddRange(newHoles);
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 홀 저장");
                        }

                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "벌크 저장 시작 - 이미지, 환불 정책, 코스, 홀 정보");
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", "골프장 처리 완료");

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessGolfClub successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessGolfClub", $"골프장 저장 실패 {ex.Message}",true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                };
            });
        }
    }
}
