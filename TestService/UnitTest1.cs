using Microsoft.Extensions.Options;
using RAGDatabaseAssistant.Infrastructure.Database;
using RAGDatabaseAssistant.Infrastructure.Services;

namespace TestService;

public class UnitTest1
{
    [Fact]
    public async Task GetDatabases_ShouldReturnOnlyEnabledDatabases()
    {
        // ARRANGE (Preparación) 
        
        // 1. Definir la lista de opciones de prueba
        var optionsList = new List<Databases>
        {
            new Databases { Name = "Postgres_Primary", ProviderType = "PostgreSQL", Enabled = true , ConnectionString = "xd"},
            new Databases { Name = "SqlServer_Archive", ProviderType = "SQLServer", Enabled = false, ConnectionString = "xd" }, // DESHABILITADA
            new Databases { Name = "MySQL_Reporting", ProviderType = "MySQL", Enabled = true , ConnectionString = "xd"},
            new Databases { Name = "SQLite_Local", ProviderType = "SQLite", Enabled = false , ConnectionString = "xd"} // DESHABILITADA
        };
        var data = new Data()
        {
            Databases = optionsList
        };

        // 2. Crear el mock de IOptions<T> usando OptionsWrapper<T>
        // OptionsWrapper es la implementación concreta de IOptions<T> que usamos para las pruebas.
        var mockOptions = new OptionsWrapper<Data>(data);

        // 3. Instanciar la clase a probar
        var dataBaseInfo = new DataBaseInfo(mockOptions);

        
        // ACT (Actuación)
        
        var result = await dataBaseInfo.GetDatabases();

        
        // ASSERT (Aserción)
        
        // 1. Verificar el número de elementos devueltos
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Solo 2 estaban habilitadas (Postgres y MySQL)

        // 2. Verificar que solo las habilitadas estén en el resultado
        Assert.All(result, item => Assert.True(item.Enabled));

        // 3. Verificar que los nombres de las bases de datos sean correctos
        var databaseNames = result.Select(d => d.Name).ToList();
        Assert.Contains("Postgres_Primary", databaseNames);
        Assert.Contains("MySQL_Reporting", databaseNames);
        Assert.DoesNotContain("SqlServer_Archive", databaseNames);
        Assert.DoesNotContain("SQLite_Local", databaseNames);
    }
}