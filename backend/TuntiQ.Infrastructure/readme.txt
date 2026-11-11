TypeGen generate
Add-Migration InitialCreate -OutputDir Persistence/Migrations -Project TuntiQ.Infrastructure -StartupProject TuntiQ.Api

dotnet tool install --global dotnet-ef
