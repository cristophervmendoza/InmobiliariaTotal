using backend_csharpcd_inmo.Structure_MVC.DAO;

var builder = WebApplication.CreateBuilder(args);

// CORS: permite Angular (ajusta orígenes si usas otros puertos/domains)
const string DevCors = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCors, policy =>
        policy.WithOrigins(
                "http://localhost:49707",
                "https://localhost:49707"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // opcional, solo si usas cookies/autenticación
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAOs
builder.Services.AddScoped<CitaDao>();
builder.Services.AddScoped<CitaHistorialDao>();
builder.Services.AddScoped<EstadoCitaDao>();
builder.Services.AddScoped<UsuarioDao>();
builder.Services.AddScoped<EstadoUsuarioDao>();
builder.Services.AddScoped<MantenimientoDao>();
builder.Services.AddScoped<EstadoMantenimientoDao>();
builder.Services.AddScoped<EstadoPropiedadDao>();
builder.Services.AddScoped<AgenteInmobiliarioDao>();
builder.Services.AddScoped<AdministradorDao>();
builder.Services.AddScoped<ClienteDao>();
builder.Services.AddScoped<TestimonioDao>();
builder.Services.AddScoped<AgendaDao>();
builder.Services.AddScoped<PostulacionDao>();
builder.Services.AddScoped<PropiedadDao>();
builder.Services.AddScoped<SolicitudPropiedadDao>();
builder.Services.AddScoped<TipoPrioridadDao>();
builder.Services.AddScoped<TipoPropiedadDao>();
builder.Services.AddScoped<FotoPropiedadDao>();
builder.Services.AddScoped<EmpresaDao>();
builder.Services.AddScoped<ReporteDao>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(DevCors); 

app.UseAuthorization();

app.MapControllers();

app.Run();
