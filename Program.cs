using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Web;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");                  //Ovaj endPoint je default i samo je prikaz da server odgovara
NBAapi.Program p = new NBAapi.Program();                //Ova linija koda sluzi da bi pitao korisnika odakle a zatim i ucitao podatke iz date datoteke
app.MapGet("/stats/player/{Name}",(string Name) => {    //ova metoda kreira endpoint koji je trazen u zadatku
    
    
    
    string resp = NBAapi.Program.json(Name);            //ovde je varijabla resp tipa string,podaci se automatski serijalizuju u JSON
                                                        //objekte kada
                                                        //se vracaju preko return



    if (resp.Length == 0)  //Slucaj kada ne postoji igrac sa prosledjenim imenom
    {
        return @"
                {
                    ""Error Response""{
                            ""Status code"": 404,
                            ""Explanation"": Player not found
                        }
                    }";
      
    }
    else
    {
        return resp;    //Vracamo vrednost koju nam vrati funkcija json
    }
});

app.Run();
