var builder = WebApplication.CreateBuilder(args);

// Register the skill
builder.Services.AddSkill<MySkill>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Map the skill endpoint
app.MapSkill();

app.Run();


// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

//app.UseHttpsRedirection();

app.Run();


public class MySkill : Skill
{
    public MySkill()
    {
        // Register an action that Copilot Studio can call
        RegisterAction("HandleCustomEvent", HandleCustomEvent);
    }

    private async Task<SkillResponse> HandleCustomEvent(SkillRequest request)
    {
        var eventName = request.Parameters.ContainsKey("eventName") ? request.Parameters["eventName"].ToString() : "unknown";
        return new SkillResponse($"Event '{eventName}' handled successfully!");
    }
}

