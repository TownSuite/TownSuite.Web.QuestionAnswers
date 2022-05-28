using System.Net;
using System.Net.Mail;
using TownSuite.Web.QuestionAnswers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string question = builder.Configuration.GetValue<string>("Question");
string expectedAnswer = builder.Configuration.GetValue<string>("Answer");

app.MapGet("/question", () => question);
app.MapPost("/answer", async (HttpContext context, Model model) =>
{
    if (string.Equals(expectedAnswer, model.Answer, StringComparison.InvariantCultureIgnoreCase)
        && string.Equals(question, model.Question, StringComparison.InvariantCultureIgnoreCase))
    {
        await Email(model, builder.Configuration.GetSection("Email").Get<EmailSettings>());
        return "Thank you";
    }

    context.Response.StatusCode = 400;
    return "Incorrect";
});

app.Run();

async Task Email(Model model, EmailSettings emailSettings)
{
    using var message = new MailMessage();
    using var smtp = new SmtpClient();
    message.From = new MailAddress(emailSettings.From);
    message.To.Add(new MailAddress(emailSettings.To));
    message.Subject = "Question Answers Correct Result";
    message.IsBodyHtml = true; //to make message body as html  
    message.Body = $@"{model.FirstName} {model.LastName}<br/>
Github Link: {model.GithubLink} <br/>
Email Address: {model.EmailAddress} <br/>
Question: {model.Question} <br/>
<br/>
Answer: <br/>
{model.Answer}
<br/>
<br/>
Source Code: <br/>
{model.SourceCode} <br/>
";
    smtp.Port = emailSettings.Port;
    smtp.Host = emailSettings.Host; //for gmail host  
    smtp.EnableSsl = true;
    smtp.UseDefaultCredentials = false;
    smtp.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);
    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
    await smtp.SendMailAsync(message);
}