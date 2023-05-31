using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PuppeteerSharp;

class Program
{
    static async Task Main(string[] args)
    {
        string webhookUrl = "https://chat.googleapis.com/v1/spaces/AAAAP2eoOa4/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=-yD9u_9y6_YIBcQ8t6zNPsIEUDCBINkyUg2nejkooGA"; // Zastąp adresem webhooku Google Chat
        string screenshotPath = "C:/zrzut_ekranu.png"; // Ścieżka, gdzie zostanie zapisany zrzut ekranu

        await WyslijWiadomoscDoGoogleChat(webhookUrl, "Wymiana butli");

        await ZrobZrzutEkranu(screenshotPath);

        await WyslijZrzutEkranuDoGoogleChat(webhookUrl, screenshotPath);

        Console.WriteLine("Proces zakończony.");

        // Możesz również usunąć zrzut ekranu po wysłaniu, jeśli nie jest już potrzebny
        // File.Delete(screenshotPath);
    }

    static async Task WyslijWiadomoscDoGoogleChat(string webhookUrl, string wiadomosc)
    {
        using (var httpClient = new HttpClient())
        {
            var tresc = new StringContent($"{{\"text\":\"{wiadomosc}\"}}");
            tresc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var odpowiedz = await httpClient.PostAsync(webhookUrl, tresc);

            if (odpowiedz.IsSuccessStatusCode)
            {
                Console.WriteLine("Wiadomość została wysłana na Google Chat!");
            }
            else
            {
                Console.WriteLine("Wystąpił błąd podczas wysyłania wiadomości na Google Chat.");
            }
        }
    }

    static async Task ZrobZrzutEkranu(string sciezkaZrzutu)
    {
        var opcjeStartowe = new LaunchOptions
        {
            ExecutablePath = "C:/Program Files/Google/Chrome/Application/chrome.exe", // Zastąp ścieżką do pliku wykonywalnego Chromium na Twoim komputerze
            Headless = true
        };

        await using (var przegladarka = await Puppeteer.LaunchAsync(opcjeStartowe))
        {
            var strona = await przegladarka.NewPageAsync();
            await strona.GoToAsync("https://w6228w05.viessmann.net:8000/en-GB/app/VI_W16/r410_measurement?form.tok_time.earliest=-4h%40h&form.tok_time.latest=now&form.tok_res=15&form.tok_t_unit=s");

            // Opóźnienie wykonania zrzutu ekranu o 10 sekund
            await Task.Delay(15000);

            var zrzutEkranuOpcje = new ScreenshotOptions
            {
                FullPage = true
            };

            await strona.ScreenshotAsync(sciezkaZrzutu, zrzutEkranuOpcje);
        }

        Console.WriteLine("Zrzut ekranu został zapisany.");
    }

    static async Task WyslijZrzutEkranuDoGoogleChat(string webhookUrl, string sciezkaZrzutu)
    {
        using (var httpClient = new HttpClient())
        using (var formularz = new MultipartFormDataContent())
        using (var strumienPliku = File.OpenRead(sciezkaZrzutu))
        {
            formularz.Add(new StreamContent(strumienPliku), "image", "zrzut_ekranu.png");

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            var odpowiedz = await httpClient.PostAsync(webhookUrl, formularz);

            if (odpowiedz.IsSuccessStatusCode)
            {
                Console.WriteLine("Zrzut ekranu został wysłany na Google Chat!");
            }
            else
            {
                Console.WriteLine("Wystąpił błąd podczas wysyłania zrzutu ekranu na Google Chat.");
            }
        }
    }
}
