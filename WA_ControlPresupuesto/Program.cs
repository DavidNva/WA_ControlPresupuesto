using WA_ControlPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using WA_ControlPresupuesto.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();//Con esto estamos creando una politica que requiere que el usuario este autenticado para poder acceder a los controladores y acciones


builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});

builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();//Para poder usar HttpContext en los servicios
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddAutoMapper(typeof(Program));

// Agrega los servicios necesarios para manejar la identidad de los usuarios, como la autenticación y la autorización
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{ //Configuración de las opciones de identidad
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireNonAlphanumeric = false;//Que no requiera caracteres especiales
    opciones.Password.RequiredLength = 6;//Que la contraseña tenga al menos 6 caracteres 
    //opciones.User.RequireUniqueEmail = true;
}).AddErrorDescriber<MensajesDeErrorIdentity>()
.AddDefaultTokenProviders();//Para personalizar los mensajes de error que se muestran al usuario, en este caso los pasamos a español

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/Usuarios/Login";
    //opciones.AccessDeniedPath = "/Usuarios/AccesoDenegado";
});//Con esto estamos configurando las cookies para que la aplicacion para autenticacion

builder.Services.AddTransient<IServicioEmail, ServicioEmail>();
//Esto no funciona builder.Services.AddAutoMapper(typeof(Program).Assembly); porque Program no es una clase, es un archivo.  Por lo tanto, hay que crear una clase vacia para que funcione
//Transient porque no comparte codigo entre distintas instancias del mismo servicio
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}")
    .WithStaticAssets();//Lo que en .net framework era o estaba en la carpeta App_Start, en .net core va en Program.cs


app.Run();
