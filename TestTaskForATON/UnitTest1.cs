using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskForATON.Controllers;
using TaskForATON.DbStuff.Models;
using TaskForATON.ViewModels;

namespace xUnitTestTaskForATON;

public class ApiTestFixture : WebApplicationFactory<Program>;

public class IntegrationTest : IClassFixture<ApiTestFixture>
{
    protected readonly HttpClient _client;

    public IntegrationTest(ApiTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }
}

public class UserControllerTest : IntegrationTest
{
    private HttpClient _client;
    private ApiTestFixture _apiTestFixture;

    public UserControllerTest(ApiTestFixture fixture) : base(fixture)
    {
        _apiTestFixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Testo()
    {
        var res = await _client.PostAsJsonAsync("Auth/Login", new
        {
            login = "admin",
            password = "MEGA_ADMIN_OF_THIS_SITE",
        });
        res.EnsureSuccessStatusCode();
        string token = await res.Content.ReadAsStringAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res2 = await _client.PostAsJsonAsync("User/Create", new CreateUserModel
        {
            Login = "lol",
            Password = "kek",
            Admin = false,
            Birthday = DateTime.Now,
            Gender = 1,
            Name = "John"
        });
        res2.EnsureSuccessStatusCode();
        var res3 = await _client.GetAsync($"User/{nameof(UserController.GetAllActiveUsers)}");
        res3.EnsureSuccessStatusCode();
        Guid[]? guids = await res3.Content.ReadFromJsonAsync<Guid[]>();
        Assert.NotNull( guids );
        Assert.Equal(2, guids.Length);
        _client.DefaultRequestHeaders.Authorization = null;
        var res4 = await _client.PostAsJsonAsync("Auth/Login", new
        {
            login = "lol",
            password = "kek"
        });
        res4.EnsureSuccessStatusCode();
        token = await res4.Content.ReadAsStringAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res5 = await _client.PostAsJsonAsync("User/Create", new CreateUserModel
        {
            Login = "kolk",
            Password = "kekr",
            Admin = false,
            Birthday = DateTime.Now,
            Gender = 1,
            Name = "Johna"
        });
        Assert.Equal(HttpStatusCode.Forbidden, res5.StatusCode);
        var res6 = await _client.PostAsJsonAsync($"User/{nameof(UserController.GetUserWithLoginAndPassword)}", new
        {
            login = "lol",
            password = "kek"
        });
        res6.EnsureSuccessStatusCode();
        var usr = await res6.Content.ReadFromJsonAsync<UserModel>();
        Assert.NotNull(usr);
        string newName = "ExtraKiller";
        var res7 = await _client.PostAsJsonAsync($"User/{nameof(UserController.ChangeName)}", new ChangeNameModel(usr.Guid, newName));
        res7.EnsureSuccessStatusCode();
        var res8 = await _client.PostAsJsonAsync($"User/{nameof(UserController.GetUserWithLoginAndPassword)}", new
        {
            login = "lol",
            password = "kek"
        });
        res8.EnsureSuccessStatusCode();

        usr = await res8.Content.ReadFromJsonAsync<UserModel>();
        Assert.NotNull(usr);
        Assert.Equal(newName, usr.Name);
        var res9 = await _client.PostAsJsonAsync("Auth/Login", new
        {
            login = "admin",
            password = "MEGA_ADMIN_OF_THIS_SITE",
        });
        token = await res9.Content.ReadAsStringAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res10 = await _client.PostAsJsonAsync($"User/{nameof(UserController.SafeDelete)}", new
        {
            Guid = usr.Guid,
        });
        res10.EnsureSuccessStatusCode();
        var res11 = await _client.PostAsJsonAsync("Auth/Login", new
        {
            login = "lol",
            password = "kek"
        });
        Assert.Equal(HttpStatusCode.Forbidden, res11.StatusCode);
        var res12 = await _client.PostAsJsonAsync($"User/{nameof(UserController.GetUserWithLogin)}", new
        {
            Str = "lol"
        });
        res12.EnsureSuccessStatusCode();

        GetUserViewModel? guv = await res12.Content.ReadFromJsonAsync<GetUserViewModel>();
        Assert.NotNull(guv);
        var res13 = await _client.PostAsJsonAsync($"User/{nameof(UserController.DangerDelete)}", new
        {
            Guid = usr.Guid
        });
        res13.EnsureSuccessStatusCode();

        var res14 = await _client.GetAsync($"User/{nameof(UserController.GetAllActiveUsers)}");
        res14.EnsureSuccessStatusCode();
        guids = await res14.Content.ReadFromJsonAsync<Guid[]>();
        Assert.NotNull(guids);
        Assert.Single(guids);

        var res15 = await _client.PostAsJsonAsync("User/Create", new CreateUserModel
        {
            Login = "lol",
            Password = "kek",
            Admin = false,
            Birthday = DateTime.Now,
            Gender = 15,
            Name = "John"
        });
        Assert.Equal(HttpStatusCode.BadRequest, res15.StatusCode);
    }
}