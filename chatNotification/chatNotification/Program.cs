using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PuppeteerSharp;

class Program
{
    static async Task Main(string[] args)
    {




        string screenshotPath = "C:/zrzut_ekranu.png"; // Ścieżka, gdzie zostanie zapisany zrzut ekranu
        string imgurClientId = "0fe6e59673311dc"; // Zastąp wartością swojego Client ID zarejestrowanego na Imgur
        string chatWebhookUrl = "https://chat.googleapis.com/v1/spaces/AAAAP2eoOa4/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=-yD9u_9y6_YIBcQ8t6zNPsIEUDCBINkyUg2nejkooGA"; // Zastąp wartością swojego URL webhooka do Chat Google

        string imagePath = "C:/zrzut_ekranu.png"; // Zastąp ścieżką do swojego pliku z obrazem

        await ZrobZrzutEkranu(screenshotPath);

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
                Console.WriteLine("Oczekiwanie na screen...");
                await Task.Delay(15000);


                var zrzutEkranuOpcje = new ScreenshotOptions
                {
                    FullPage = true
                };

                await strona.ScreenshotAsync(sciezkaZrzutu, zrzutEkranuOpcje);
            }

            Console.WriteLine("Zrzut ekranu został zapisany.");
        }



        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {imgurClientId}");

            byte[] imageData;
            using (var fileStream = File.OpenRead(imagePath))
            {
                imageData = new byte[fileStream.Length];
                await fileStream.ReadAsync(imageData, 0, imageData.Length);
            }

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(imageData), "image", "image.jpg");

            var response = await httpClient.PostAsync("https://api.imgur.com/3/image", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Pobranie linku do przesłanego obrazu z Imgur
                var imgurResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ImgurResponse>(responseContent);
                string imageUrl = imgurResponse.Data.Link;

                // Wysłanie wiadomości na Chat Google z linkiem do obrazu
                using (HttpClient chatClient = new HttpClient())
                {
                    var chatMessage = new
                    {
                        text = $"WYCIEK R410!!: {imageUrl}"
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage);
                    var chatContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage chatResponse = await chatClient.PostAsync(chatWebhookUrl, chatContent);

                    if (chatResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Wiadomość została wysłana z linkiem do obrazu.");
                    }
                    else
                    {
                        Console.WriteLine("Wystąpił błąd podczas wysyłania wiadomości na Chat Google.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Wystąpił błąd podczas przesyłania obrazu do Imgur.");
            }
        }
    }

    class ImgurResponse
    {
        public ImgurData Data { get; set; }
    }

    class ImgurData
    {
        public string Link { get; set; }
    }
}
