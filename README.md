# Rest.API.Translator
 
`Rest.API.Translator` is a small library that translate interface to valid rest api calls. So instead of using url for each methods, you only use interface and expression to build a valid call to your rest api.
 
## Code example 
Imagine you have a rest api with a following controller.
```cshap
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DbController : ControllerBase
    {
     [HttpGet]
     public string GetName(string firstName, string lastName){
     return firstName + lastName;
     }
     
     [HttpGet]
     public async Task<string> GetLastName(){
     .... your code
     }
     
     [HttpPost]
     public void SaveUser(User user){
      .... your code
     }
    }
```

With `Rest.API.Translator` you could build an interface to the current controller above with ease and start your call.

First build an interface to the current restapi.
And lets assume that the `baseUrl` for our rest api is `http://test.com` 

```csharp
[Route(relativeUrl: "api/")]
public interface IDbController{
string GetName(string firstName, string lastName);

Task<string> GetLastName();

// another example to GetLastName using a diffrent method name and none async method
[Route(relativeUrl: "GetLastName")]
string GetlstName();

[Route(httpMethod: MethodType.JSONPOST)]
Task SaveUser(User user);
}
```
Now we would like to use the current interface to make a call for our rest api using `Rest.API.Translator`
```csharp
    using (var db = new APIController<IDbController>(_baseUrl)){
      var name = db.Execute(x=> x.GetName("alen", "toma"));
      // here is how you handle async methods
      var lstName= db.GetLastName().Await();
      // or simple 
      var lstName= await db.GetLastName();
    }
```
