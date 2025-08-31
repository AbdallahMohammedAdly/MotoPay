using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;

namespace AutoLease.Web.Pages.Cars
{
    [Authorize]
    public class BrandShowcaseModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ICarRepository _carRepository;
        private readonly IOfferRepository _offerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandShowcaseModel> _logger;

        public BrandShowcaseModel(IMediator mediator, ICarRepository carRepository, IOfferRepository offerRepository, IMapper mapper, ILogger<BrandShowcaseModel> logger)
        {
            _mediator = mediator;
            _carRepository = carRepository;
            _offerRepository = offerRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public PagedResult<CarWithOfferDto> PagedCars { get; set; } = new();
        public Dictionary<string, List<CarWithOfferDto>> CarsByBrand { get; set; } = new();
        public List<string> AllBrands { get; set; } = new();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? SelectedBrand { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsAvailable { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasOffer { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "Make";

        [BindProperty(SupportsGet = true)]
        public bool SortDescending { get; set; } = false;

        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                       !string.IsNullOrEmpty(SelectedBrand) || 
                                       Year.HasValue || 
                                       MinPrice.HasValue || 
                                       MaxPrice.HasValue || 
                                       IsAvailable.HasValue ||
                                       HasOffer.HasValue;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCarsAsync();
            
            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Partial("_BrandShowcaseGrid", this);
            }
            
            return Page();
        }

        public async Task<IActionResult> OnGetFilterAsync()
        {
            await LoadCarsAsync();
            return Partial("_BrandShowcaseGrid", this);
        }

        private async Task LoadCarsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving cars with offers for brand showcase");

                // Get filtered cars
                var carsQuery = new GetFilteredCarsQuery
                {
                    SearchTerm = SearchTerm,
                    Make = SelectedBrand,
                    Year = Year,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    IsAvailable = IsAvailable,
                    PageNumber = 1, // Get all for brand list
                    PageSize = 1000, // Get all for brand list
                    SortBy = "Make",
                    SortDescending = false
                };

                var carsResult = await _mediator.Send(carsQuery);
                var allCars = carsResult.Items.ToList();

                // Get all brands for filter
                AllBrands = allCars.Select(c => c.Make).Distinct().OrderBy(m => m).ToList();

                // Get all active offers
                var offersQuery = new GetFilteredOffersQuery 
                { 
                    IsActive = true,
                    PageSize = 1000 // Get all offers
                };
                var offersResult = await _mediator.Send(offersQuery);
                var activeOffers = offersResult.Items.Where(o => !o.IsExpired).ToList();

                // Create a dictionary of car offers
                var carOffers = activeOffers.GroupBy(o => o.CarId).ToDictionary(g => g.Key, g => g.ToList());

                // Create cars with offers
                var allCarsWithOffers = new List<CarWithOfferDto>();
                foreach (var car in allCars)
                {
                    var carWithOffer = new CarWithOfferDto
                    {
                        Car = car,
                        Offers = carOffers.ContainsKey(car.Id) ? carOffers[car.Id] : new List<OfferDto>()
                    };
                    allCarsWithOffers.Add(carWithOffer);
                }

                // Apply offer filter if needed
                if (HasOffer.HasValue)
                {
                    allCarsWithOffers = HasOffer.Value 
                        ? allCarsWithOffers.Where(c => c.HasActiveOffer).ToList()
                        : allCarsWithOffers.Where(c => !c.HasActiveOffer).ToList();
                }

                // Sort cars
                allCarsWithOffers = SortCars(allCarsWithOffers, SortBy, SortDescending);

                // Group by brand for display
                var carGroups = allCarsWithOffers.GroupBy(c => c.Car.Make);
                foreach (var group in carGroups)
                {
                    CarsByBrand[group.Key] = group.ToList();
                }

                // Apply pagination
                var totalCount = allCarsWithOffers.Count;
                var pageSize = 12;
                var pagedItems = allCarsWithOffers
                    .Skip((PageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                PagedCars = new PagedResult<CarWithOfferDto>(pagedItems, totalCount, PageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cars for brand showcase");
                PagedCars = new PagedResult<CarWithOfferDto>();
            }
        }

        private List<CarWithOfferDto> SortCars(List<CarWithOfferDto> cars, string sortBy, bool descending)
        {
            return sortBy switch
            {
                "Make" => descending ? cars.OrderByDescending(c => c.Car.Make).ToList() : cars.OrderBy(c => c.Car.Make).ToList(),
                "Model" => descending ? cars.OrderByDescending(c => c.Car.Model).ToList() : cars.OrderBy(c => c.Car.Model).ToList(),
                "Year" => descending ? cars.OrderByDescending(c => c.Car.Year).ToList() : cars.OrderBy(c => c.Car.Year).ToList(),
                "Price" => descending ? cars.OrderByDescending(c => c.BestOffer?.DiscountedPrice ?? c.Car.Price).ToList() : cars.OrderBy(c => c.BestOffer?.DiscountedPrice ?? c.Car.Price).ToList(),
                "Discount" => descending ? cars.OrderByDescending(c => c.BestOffer?.DiscountPercentage ?? 0).ToList() : cars.OrderBy(c => c.BestOffer?.DiscountPercentage ?? 0).ToList(),
                _ => cars.OrderByDescending(c => c.Car.CreatedAt).ToList()
            };
        }

        public string GetSortUrl(string sortBy)
        {
            var routeValues = new Dictionary<string, string?>
            {
                ["SearchTerm"] = SearchTerm,
                ["SelectedBrand"] = SelectedBrand,
                ["Year"] = Year?.ToString(),
                ["MinPrice"] = MinPrice?.ToString(),
                ["MaxPrice"] = MaxPrice?.ToString(),
                ["IsAvailable"] = IsAvailable?.ToString(),
                ["HasOffer"] = HasOffer?.ToString(),
                ["PageNumber"] = "1",
                ["SortBy"] = sortBy,
                ["SortDescending"] = (sortBy == SortBy && !SortDescending).ToString()
            };

            var query = string.Join("&", routeValues
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

            return string.IsNullOrEmpty(query) ? "" : "?" + query;
        }

        public string GetPageUrl(int pageNumber)
        {
            var routeValues = new Dictionary<string, string?>
            {
                ["SearchTerm"] = SearchTerm,
                ["SelectedBrand"] = SelectedBrand,
                ["Year"] = Year?.ToString(),
                ["MinPrice"] = MinPrice?.ToString(),
                ["MaxPrice"] = MaxPrice?.ToString(),
                ["IsAvailable"] = IsAvailable?.ToString(),
                ["HasOffer"] = HasOffer?.ToString(),
                ["PageNumber"] = pageNumber.ToString(),
                ["SortBy"] = SortBy,
                ["SortDescending"] = SortDescending.ToString()
            };

            var query = string.Join("&", routeValues
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

            return string.IsNullOrEmpty(query) ? "" : "?" + query;
        }
    }

    public class CarWithOfferDto
    {
        public CarDto Car { get; set; } = null!;
        public List<OfferDto> Offers { get; set; } = new();
        public bool HasActiveOffer => Offers.Any();
        public OfferDto? BestOffer => Offers.OrderByDescending(o => o.DiscountPercentage).FirstOrDefault();
    }
}