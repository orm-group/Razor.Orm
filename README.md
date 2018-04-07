Razor.Orm - Um micro ORM baseado no Razor
========================================

Para baixar e instalar você deve acessar o link abaixo:

[Razor.Orm NuGet library](https://www.nuget.org/packages/Razor.Orm)

Para uma melhor integração com o LightInject foi feito o DaoCompositionRoot onde você pode baixar e instalar acessando o link abaixo:

[LightInject.Razor.Orm NuGet library](https://www.nuget.org/packages/LightInject.Razor.Orm)

# Guia
- [Primeiros passos](#primeiros-passos)

# Primeiros passos

Para comerçar é necessario montar uma estrutura de camada de persistencia, precisamos criar classes Dto(classes POCO) que serão responsaveis pelo trafego de dados. 

```csharp
namespace MyProject
{
    public class PeopleFilterDto
    {
        public string LikeFirstName { get; set; }
        public long[] EmailPromotionOptions { get; set; }
    }

    public class PeopleDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; }
    }
}
```

Em seguida devemos criar uma interface que contenha o layout que será implementado pelo Razor.Orm para aquele determinado Dao

```csharp
namespace MyProject
{
    public interface IPeopleDao
    {
        IEnumerable<PeopleDto> GetAllPeople(PeopleFilterDto dto);
    }
}
```

Agora criaremos um arquivo 'cshtml' para cada metodo que foi definido na interface anteriormente, é necessario que esses arquivos fiquem no mesmo diretorio onde está a interface e que a 'Ação de Compilação' seja ajustada para opção 'Recurso inserido'

```cshtml
@using Razor.Orm.Template
@using MyProject
@inherits SqlTemplate<PeopleFilterDto, PeopleDto>
SELECT [BusinessEntityID] @As(e => e.Id)
      ,[FirstName]
      ,[ModifiedDate] @As(e => e.Date)
    FROM [Person].[Person]
    @using (Smart.Where)
    {
        if (!string.IsNullOrEmpty(Model.LikeFirstName))
        {
            <text>[FirstName] LIKE @Par($"%{Model.LikeFirstName}%")</text>
        }
        if (Model.EmailPromotionOptions != null && Model.EmailPromotionOptions.Length > 0)
        {
            Smart.And();
            <text>[EmailPromotion] @In(Model.EmailPromotionOptions)</text>
        }
    }
```

Nesse momento é necessario extender a classe DaoFactory criando sua propia fabrica de Daos

```csharp
namespace MyProject
{
    public class MyDaoFactory : DaoFactory
    {
        protected override void Setup()
        {
            Define<IPeopleDao>();
        }
    }
}
```

Agora está tudo pronto pra rodar o projeto

```csharp
namespace MyProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var myDaoFactory = new MyDaoFactory();
        
            using (var connection = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True"))
            {
                connection.Open();

                var dao = myDaoFactory.CreateDao<IPeopleDao>(connection);

                foreach (var item in dao.GetAllPeople(new PeopleFilterDto()
                {
                    LikeFirstName = "Ken",
                    EmailPromotionOptions = new long[] { 0, 1 }
                }))
                {
                    Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
                }
            }
        }
    }
}
```

A busca realizada vai ser essa onde '@p0' é igual '%Ken%'

```sql
SELECT [BusinessEntityID] as 'Id'
      ,[FirstName]
      ,[ModifiedDate] as 'Date'
    FROM [Person].[Person]
 WHERE [FirstName] LIKE @p0 AND [EmailPromotion] in (0,1)
```

E a saída no console vai ser essa

```txt
Id: 10300, Date: 27/12/2013 00:00:00, FirstName: Mackenzie
Id: 15145, Date: 05/08/2012 00:00:00, FirstName: Kendra
Id: 15137, Date: 22/04/2013 00:00:00, FirstName: Kendra
Id: 2618, Date: 28/11/2013 00:00:00, FirstName: Kenneth
Id: 8832, Date: 01/03/2014 00:00:00, FirstName: Mackenzie
Id: 10361, Date: 14/07/2013 00:00:00, FirstName: Mackenzie
Id: 2625, Date: 27/03/2014 00:00:00, FirstName: Kenneth
Id: 15163, Date: 13/11/2013 00:00:00, FirstName: Kendra
...
```
