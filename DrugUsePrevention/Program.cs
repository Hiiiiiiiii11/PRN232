using System.Text; // Thêm dòng này ở đầu file
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.IRepository;
using Repositories.IRepository.Admins;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Categories;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Courses;
using Repositories.IRepository.NewsArticles;
using Repositories.IRepository.Tags;
using Repositories.IRepository.Users;
using Repositories.Repository;
using Repositories.Repository.Admins;
using Repositories.Repository.Appointments;
using Repositories.Repository.Categories;
using Repositories.Repository.Consultants;
using Repositories.Repository.Courses;
using Repositories.Repository.NewsArticles;
using Repositories.Repository.Tags;
using Repositories.Repository.Users;
using Services.IService;
using Services.MailUtils;
using Services.Service;

// ... các using khác

namespace DrugUsePrevention
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<DrugUsePreventionDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnectionString")
                )
            );

            var mailsettings = builder.Configuration.GetSection("MailSettings"); // đọc config
            builder.Services.Configure<MailSettings>(mailsettings);

            // Dependency Injection
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddTransient<ISendMailService, SendMailService>();
            builder.Services.AddScoped<
                ICourseRegistrationRepository,
                CourseRegistrationRepository
            >();
            builder.Services.AddScoped<ICourseContentRepository, CourseContentRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IConsultantRepository, ConsultantRepository>();
            builder.Services.AddScoped<IConsultantService, ConsultantService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IConsultantUserService, ConsultantUserService>();
            builder.Services.AddScoped<IConsultantUserRepository, ConsultantUserRepository>();
            // Add Controllers
            builder.Services.AddControllers();

            // Add Razor Pages - PHẢI ĐẶT TRƯỚC builder.Build()
            builder.Services.AddRazorPages();

            // Thêm Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo { Title = "DrugUsePrevention API", Version = "v1" }
                );

                // Thêm cấu hình xác thực JWT vào Swagger
                c.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer abcdef12345\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                    }
                );

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            Array.Empty<string>()
                        },
                    }
                );
            });

            // JWT Authentication
            builder
                .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                        ValidAudience = builder.Configuration["AppSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)
                        ),
                        ValidateIssuerSigningKey = true,
                    };
                });

            builder.Services.AddAuthorization();

            // BUILD APP - SAU DÒNG NÀY KHÔNG ĐƯỢC THÊM SERVICES NỮA
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // 👉 Cho phép Swagger UI luôn bật (không cần check Development)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrugUsePrevention API v1");
            });


            // Middleware pipeline
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Thêm dòng này để serve static files cho Razor Pages

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Mapping
            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}
