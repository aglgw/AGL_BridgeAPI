using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static AGL.Api.API_Template.Models.OAPI.OAPI;

namespace AGL.Api.API_Template.Services
{
    public class GolfClubService : IGolfClubService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;

        public GolfClubService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
        }


        public async Task<IDataResult> PostGolfClub(GolfClubInfo request, string supplierCode)
        {
            return await ProcessGolfClub(request, supplierCode, request.GolfclubCode);
        }

        public async Task<IDataResult> PutGolfClub(GolfClubInfo request, string supplierCode)
        {
            return await ProcessGolfClub(request, supplierCode, request.GolfclubCode);
        }

        public async Task<IDataResult> GetGolfClub(string supplierCode, string golfClubCode)
        {
            if (string.IsNullOrEmpty(golfClubCode))
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "startDate or ) is invalid", null);
            }

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
                    .Where(g => g.Supplier == supplier);

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
                    return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "GolfClubs Not Found", null);
                }

                var golfClubDtos = existingGolfclubs.Select(golfClub => new GolfClubInfo
                {
                    GolfclubCode = golfClub.GolfClubCode,
                    GolfclubName = golfClub.GolfClubName,
                    CountryCode = golfClub.CountryCode,
                    Currency = golfClub.Currency,
                    Description = golfClub.Description,
                    Address = golfClub.Address,
                    Latitude = golfClub.Latitude?.ToString(),
                    Longitude = golfClub.Longitude?.ToString(),
                    Phone = golfClub.Phone,
                    Fax = golfClub.Fax,
                    Email = golfClub.Email,
                    Homepage = golfClub.Homepage,
                    TotalHoleCount = golfClub.TotalHoleCount,
                    TotalCourseCount = golfClub.Courses.Count,
                    IsGuestInfoRequired = golfClub.isGuestInfoRequired,
                    Image = golfClub.GolfClubImages.Select(img => new Images
                    {
                        id = img.Idx,
                        Url = img.Url,
                        Title = img.Title,
                        Description = img.ImageDescription
                    }).ToList(),
                    RefundPolicy = golfClub.RefundPolicies.Select(rp => new RefundPolicy { 
                        RefundDate = rp.RefundDate,
                        RefundFee = rp.RefundFee,
                        RefundUnit = rp.RefundUnit,
                    }).ToList(),
                    Course = golfClub.Courses.Select(c => new Course
                    {
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        CourseHoleCount = c.CourseHoleCount,
                        StartHole = c.StartHole,
                    }).ToList(),
                    HoleInfo = golfClub.Holes.Select(h => new HoleInfo
                    {
                        HoleNumber = h.HoleNumber,
                        HoleName = h.HoleName,
                        Par = h.Par,
                        DistanceUnit = h.DistanceUnit,
                        Distance = h.Distance
                    }).ToList()
                }).ToList();

                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "GolfClub List successfully", golfClubDtos);
            }
            catch (Exception ex)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }


        private async Task<IDataResult> ProcessGolfClub(GolfClubInfo request, string supplierCode, string golfclubCode)
        {

            if (string.IsNullOrEmpty(request.GolfclubCode))
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "startDate or ) is invalid", null);
            }

            try
            {
                // 공급사 코드로 공급사 ID 조회
                OAPI_Supplier supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                int supplierId = supplier.SupplierId;

                // 모든 관련 데이터를 미리 조회 골프장,이미지,환불정책,코스,홀
                var existingGolfclub = await _context.GolfClubs
                    .Include(g => g.GolfClubImages)
                    .Include(g => g.RefundPolicies)
                    .Include(g => g.Courses)
                    .Include(g => g.Holes)
                    .FirstOrDefaultAsync(g => g.SupplierId == supplierId && g.GolfClubCode == golfclubCode);

                List<OAPI_GolfClubImage> existingImages = existingGolfclub?.GolfClubImages.ToList() ?? new List<OAPI_GolfClubImage>();
                List<OAPI_GolfClubRefundPolicy> existingRefundPolicies = existingGolfclub?.RefundPolicies.ToList() ?? new List<OAPI_GolfClubRefundPolicy>();
                List<OAPI_GolfClubCourse> existingCourses = existingGolfclub?.Courses.ToList() ?? new List<OAPI_GolfClubCourse>();
                List<OAPI_GolfClubHole> existingHoles = existingGolfclub?.Holes.ToList() ?? new List<OAPI_GolfClubHole>();

                // 골프장 정보
                int golfClubId;
                if (existingGolfclub != null)
                {
                    golfClubId = existingGolfclub.GolfClubId;
                    // 기존 골프장 정보 업데이트
                    existingGolfclub.GolfClubName = request.GolfclubName;
                    existingGolfclub.CountryCode = request.CountryCode;
                    existingGolfclub.Currency = request.Currency;
                    existingGolfclub.Description = request.Description;
                    existingGolfclub.Address = request.Address;
                    existingGolfclub.Latitude = request.Latitude;
                    existingGolfclub.Longitude = request.Longitude;
                    existingGolfclub.Phone = request.Phone;
                    existingGolfclub.Fax = request.Fax;
                    existingGolfclub.Email = request.Email;
                    existingGolfclub.Homepage = request.Homepage;
                    existingGolfclub.TotalHoleCount = request.TotalHoleCount;
                    existingGolfclub.TotalCourseCount = request.TotalCourseCount;
                    existingGolfclub.isGuestInfoRequired = request.IsGuestInfoRequired;
                    existingGolfclub.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    var newGolfClub = new OAPI_GolfClub
                    {
                        SupplierId = supplierId,
                        GolfClubCode = request.GolfclubCode,
                        GolfClubName = request.GolfclubName,
                        CountryCode = request.CountryCode,
                        Currency = request.Currency,
                        Description = request.Description,
                        Address = request.Address,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        Phone = request.Phone,
                        Fax = request.Fax,
                        Email = request.Email,
                        Homepage = request.Homepage,
                        TotalHoleCount = request.TotalHoleCount,
                        TotalCourseCount = request.TotalCourseCount,
                        isGuestInfoRequired = request.IsGuestInfoRequired,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.GolfClubs.Add(newGolfClub);
                    await _context.SaveChangesAsync();
                    golfClubId = newGolfClub.GolfClubId;
                    existingGolfclub = newGolfClub;
                }

                // 벌크 저장을 위한 리스트 준비
                var newImages = new List<OAPI_GolfClubImage>();
                var newRefundPolicies = new List<OAPI_GolfClubRefundPolicy>();
                var newCourses = new List<OAPI_GolfClubCourse>();
                var newHoles = new List<OAPI_GolfClubHole>();

                // 이미지 정보 저장 (유효성 체크)
                if (request.Image != null)
                {
                    foreach (var image in request.Image)
                    {
                        var existingImage = existingImages.FirstOrDefault(i => i.Idx == image.id);
                        if (existingImage != null)
                        {
                            existingImage.Url = image.Url;
                            existingImage.Title = image.Title;
                            existingImage.ImageDescription = image.Description;
                            existingImage.UpdatedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            var newImage = new OAPI_GolfClubImage
                            {
                                GolfClubId = golfClubId,
                                Idx = image.id,
                                Url = image.Url,
                                Title = image.Title,
                                ImageDescription = image.Description,
                                CreatedDate = DateTime.UtcNow
                            };
                            newImages.Add(newImage);
                        }
                    }
                }

                // 환불 정책 정보 저장 (유효성 체크)
                if (request.RefundPolicy != null)
                {
                    foreach (var refundPolicy in request.RefundPolicy)
                    {
                        var existingPolicy = existingRefundPolicies.FirstOrDefault(rp => rp.RefundDate == refundPolicy.RefundDate);
                        if (existingPolicy != null)
                        {
                            //existingPolicy.RefundHour = refundPolicy.RefundHour;
                            existingPolicy.RefundFee = refundPolicy.RefundFee;
                            existingPolicy.RefundHour = refundPolicy.RefundHour;
                            existingPolicy.RefundUnit = refundPolicy.RefundUnit;
                            existingPolicy.UpdatedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            var newRefundPolicy = new OAPI_GolfClubRefundPolicy
                            {
                                GolfClubId = golfClubId,
                                RefundDate = refundPolicy.RefundDate,
                                RefundHour = refundPolicy.RefundHour,
                                RefundFee = refundPolicy.RefundFee,
                                RefundUnit = refundPolicy.RefundUnit,
                                CreatedDate = DateTime.UtcNow
                            };
                            newRefundPolicies.Add(newRefundPolicy);
                        }
                    }
                }

                // 코스 정보 저장 (유효성 체크)
                if (request.Course != null)
                {
                    foreach (var course in request.Course)
                    {
                        var existingCourse = existingCourses.FirstOrDefault(c => c.CourseCode.ToString() == course.CourseCode.ToString());
                        if (existingCourse != null)
                        {
                            existingCourse.CourseName = course.CourseName;
                            existingCourse.UpdatedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            var newCourse = new OAPI_GolfClubCourse
                            {
                                GolfClubId = golfClubId,
                                CourseCode = course.CourseCode,
                                CourseName = course.CourseName,
                                CreatedDate = DateTime.UtcNow,
                            };
                            newCourses.Add(newCourse);
                        }
                    }
                }

                // 홀 정보 저장 (유효성 체크)
                if (request.HoleInfo != null)
                {
                    foreach (var hole in request.HoleInfo)
                    {
                        var existingHole = existingHoles.FirstOrDefault(h => h.HoleNumber.ToString() == hole.HoleNumber.ToString());
                        if (existingHole != null)
                        {
                            existingHole.Par = hole.Par;
                            existingHole.DistanceUnit = hole.DistanceUnit;
                            existingHole.Distance = hole.Distance;
                            existingHole.UpdatedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            var newHole = new OAPI_GolfClubHole
                            {
                                GolfClubId = golfClubId,
                                HoleNumber = hole.HoleNumber,
                                Par = hole.Par,
                                DistanceUnit = hole.DistanceUnit,
                                Distance = hole.Distance,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = null
                            };
                            newHoles.Add(newHole);
                        }
                    }
                }

                // 벌크로 새로운 항목 추가
                if (newImages.Any()) _context.GolfClubImages.AddRange(newImages);
                if (newRefundPolicies.Any()) _context.GolfClubRefundPolicies.AddRange(newRefundPolicies);
                if (newCourses.Any()) _context.GolfClubCourses.AddRange(newCourses);
                if (newHoles.Any()) _context.GolfClubHoles.AddRange(newHoles);

                await _context.SaveChangesAsync();

                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessGolfClub successfully", null);
            }
            catch (Exception ex)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }
    }
}
