        using E_Commerce.DAL.Repository;
        using Microsoft.AspNetCore.Identity;
        using Microsoft.AspNetCore.Identity.UI.Services;
        using Microsoft.EntityFrameworkCore;
        using MohamedHussien.DAL.Repository.IRepository;
        using MohamedHussien.Data;
        using MohamedHussien.Utilities;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Strip")); 


        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddMvc()
        .AddRazorPagesOptions(options =>
        {
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        StripeConfiguration.ApiKey = builder.Configuration.GetSection("Strip:SecretKey").Get<string>(); 
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
