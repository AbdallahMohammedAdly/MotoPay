using AutoLease.Application.Interfaces;
using AutoLease.Application.Mappings;
using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoLease.Infrastructure.Services;
using AutoLease.Persistence.Data;
using AutoLease.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddRazorPages()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Configure form options for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50L * 1024L * 1024L; // 50 MB total
    options.MultipartHeadersLengthLimit = 64 * 1024; // 64 KB
});

// Add Entity Framework
builder.Services.AddDbContext<AutoLeaseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AutoLeaseDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Add AutoMapper
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Add MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(AutoLease.Application.Features.Cars.Commands.CreateCarCommand).Assembly);
});

// Add repositories
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ISalesAgentRepository, SalesAgentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferApplicationRepository, OfferApplicationRepository>();

// Add services
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Localization: supported cultures
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
// Use cookie provider first so culture is retained
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AutoLeaseDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Ensure database is created
        context.Database.EnsureCreated();

        // Ensure CarInterests table exists (for SQLite dev env without migrations)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS CarInterests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CarId INTEGER NOT NULL,
                    UserId TEXT NOT NULL,
                    PreferredCallTime TEXT NOT NULL,
                    Notes TEXT NULL,
                    DocumentPaths TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    CONSTRAINT FK_CarInterests_Cars FOREIGN KEY (CarId) REFERENCES Cars(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_CarInterests_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS IX_CarInterests_CarId ON CarInterests(CarId);
                CREATE INDEX IF NOT EXISTS IX_CarInterests_UserId ON CarInterests(UserId);
                CREATE INDEX IF NOT EXISTS IX_CarInterests_CreatedAt ON CarInterests(CreatedAt);
            ");
        }
        catch { /* no-op for providers that already handle schema */ }
        
        // Seed roles, admin user, and sample data
        await SeedDataAsync(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

static async Task SeedDataAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AutoLeaseDbContext context)
{
    // Seed roles
    string[] roleNames = { "Client", "SalesAgent" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Seed admin user
    var adminEmail = "admin@autolease.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new User(adminEmail, "Admin", "User", UserRole.SalesAgent);
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "SalesAgent");
        }
    }

    // Seed sample cars for testing pagination
    if (!context.Cars.Any())
    {
        var sampleCars = new List<Car>
        {
            new Car("Toyota", "Camry", 2023, "White", "1HGBH41JXMN109186", 25000, "Reliable sedan with excellent fuel economy", "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400"),
            new Car("Honda", "Civic", 2022, "Blue", "2HGFB2F59EH542316", 22000, "Compact car perfect for city driving", "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400"),
            new Car("BMW", "X5", 2023, "Black", "5UXCR6C0XL9B12345", 55000, "Luxury SUV with premium features", "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400"),
            new Car("Mercedes", "C-Class", 2022, "Silver", "WDDGF4HB1EA123456", 45000, "Premium sedan with advanced technology", "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400"),
            new Car("Audi", "A4", 2023, "Red", "WAUENAF41KN123456", 42000, "Sport sedan with quattro all-wheel drive", "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400"),
            new Car("Ford", "F-150", 2022, "Gray", "1FTEW1E50MFA12345", 35000, "America's best-selling truck", "https://images.unsplash.com/photo-1594736797933-d0b22e8b6e4a?w=400"),
            new Car("Tesla", "Model 3", 2023, "White", "5YJ3E1EA4KF123456", 40000, "Electric sedan with autopilot", "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=400"),
            new Car("Chevrolet", "Silverado", 2022, "Blue", "1GCRYDED4KZ123456", 38000, "Heavy-duty pickup truck", "https://images.unsplash.com/photo-1594736797933-d0b22e8b6e4a?w=400"),
            new Car("Nissan", "Altima", 2023, "Black", "1N4AL3AP4KC123456", 26000, "Midsize sedan with CVT transmission", "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400"),
            new Car("Hyundai", "Elantra", 2022, "White", "KMHL14JA4MA123456", 21000, "Compact sedan with warranty", "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400"),
            new Car("Kia", "Sorento", 2023, "Green", "5XYP34HC2KG123456", 33000, "3-row SUV for families", "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400"),
            new Car("Subaru", "Outback", 2022, "Blue", "4S4BSANC1K3123456", 30000, "All-wheel drive wagon", "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400"),
            new Car("Mazda", "CX-5", 2023, "Red", "JM3KFBCM1K0123456", 28000, "Compact SUV with style", "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400"),
            new Car("Volkswagen", "Jetta", 2022, "Silver", "3VWC57BU4KM123456", 24000, "German engineering at its best", "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400"),
            new Car("Jeep", "Wrangler", 2023, "Orange", "1C4HJXDG4KW123456", 36000, "Off-road capable SUV", "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400")
        };

        context.Cars.AddRange(sampleCars);
        await context.SaveChangesAsync();
    }

    // Seed sample offers for testing pagination
    if (!context.Offers.Any())
    {
        var cars = context.Cars.Take(10).ToList(); // Get first 10 cars for offers
        if (cars.Any())
        {
            var sampleOffers = new List<Offer>
            {
                new Offer("Spring Sale - Toyota Camry", "Special discount on reliable Toyota Camry", 25000, 22000, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(20), "Limited time offer", 5, cars[0].Id),
                new Offer("Summer Deal - Honda Civic", "Great deal on fuel-efficient Honda Civic", 22000, 19500, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(25), "While supplies last", 3, cars[1].Id),
                new Offer("Luxury Special - BMW X5", "Premium BMW X5 at reduced price", 55000, 50000, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(15), "Financing available", 2, cars[2].Id),
                new Offer("Executive Package - Mercedes C-Class", "Mercedes C-Class with premium features", 45000, 41000, DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(22), "Includes extended warranty", 4, cars[3].Id),
                new Offer("Sport Edition - Audi A4", "Audi A4 with sport package", 42000, 38500, DateTime.UtcNow.AddDays(-12), DateTime.UtcNow.AddDays(18), "Quattro all-wheel drive", 3, cars[4].Id),
                new Offer("Truck Month - Ford F-150", "America's favorite truck on sale", 35000, 32000, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(23), "Work truck special", 6, cars[5].Id),
                new Offer("Electric Future - Tesla Model 3", "Tesla Model 3 with autopilot", 40000, 37000, DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(10), "Includes charging credits", 4, cars[6].Id),
                new Offer("Heavy Duty - Chevrolet Silverado", "Silverado for serious work", 38000, 35500, DateTime.UtcNow.AddDays(-6), DateTime.UtcNow.AddDays(24), "Towing package included", 5, cars[7].Id),
                new Offer("Midsize Comfort - Nissan Altima", "Comfortable Nissan Altima", 26000, 23500, DateTime.UtcNow.AddDays(-9), DateTime.UtcNow.AddDays(21), "CVT transmission", 4, cars[8].Id),
                new Offer("Value Pick - Hyundai Elantra", "Great value Hyundai Elantra", 21000, 19000, DateTime.UtcNow.AddDays(-11), DateTime.UtcNow.AddDays(19), "10-year warranty", 7, cars[9].Id)
            };

            context.Offers.AddRange(sampleOffers);
            await context.SaveChangesAsync();
        }
    }
}
