using Dj_listens.Hubs; // Dodaj namespace gdje je PartyHub

var builder = WebApplication.CreateBuilder(args);

// Dodaj servise u kontejner
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSignalR(); // Dodaj SignalR servis

var app = builder.Build();

// Konfiguracija HTTP pipeline-a
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/ErrorRedirect"); // Tvoja custom error stranica
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapiraj SignalR hub
app.MapHub<PartyHub>("/partyHub");

app.Run();
