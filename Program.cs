using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;


namespace SpaceShuttleLaunch
{
    public class SpaceportInfo
    {
        public string Spaceport { get; set; }
        public int BestDay { get; set; }
        public int DistanceKm { get; set; }
        public string NoPerfectDayMessage(string language)
        {
            switch (language)
            {
                case "E":
                    return $"{Spaceport}, no appropriate day";
                case "G":
                    return $"{Spaceport}, kein perfekt Tag";
                default:
                    return $"{Spaceport}, no appropriate day";
            }
        }
        public string ResultMessage(string language)
        {
            switch (language)
            {
                case "E":
                    return $"The best combination of day and location for space shuttle launch is: {Spaceport}, July {BestDay}";
                case "G":
                    return $"Die beste Kombination aus Tag und Location für den Start des Space Shuttles ist: {Spaceport}, Juli {BestDay}";
                default:
                    return $"The best combination of day and location for space shuttle launch is: {Spaceport}, July {BestDay}";
            }
        }
    }
    class Program
    {
        public class LanguageProperties
        {
            public string FolderNotFound { get; set; }
            public string MailSubject { get; set; }
            public string SentSuccessfuly { get; set; }
            public string SentFailed { get; set; }
            public string NoDay { get; set; }
            public string NameNotFound { get; set; }
            public string ListEmpty { get; set; }
            public string FolderName { get; set; }
            public string SenderEmail { get; set; }
            public string Password { get; set; }
            public string ReceiverEmail { get; set; }
            public string InvalidEmail { get; set; }
            public string EmailSending { get; set; }
            public string HeaderFile { get; set; }

        }
        private static readonly LanguageProperties EnglishProperties = new LanguageProperties
        {
            FolderNotFound = "The folder does not exist. Please try again!",
            MailSubject = "Space shuttle launch report",
            SentSuccessfuly = "Email sent successfully!",
            SentFailed = "Failed to send email.",
            NoDay = "There is no appropriate day for space shuttle launch.",
            NameNotFound = "Error: There is no Spaceport with that name! There is no specified distance to the equator.",
            ListEmpty = "List is empty!",
            FolderName = "Please enter the folder name:",
            SenderEmail = "Please enter the sender email address:",
            Password = "Please enter the password:",
            ReceiverEmail = "Please enter the receiver email address:",
            InvalidEmail="The email is not valid!",
            EmailSending = "The email is being sent...",
            HeaderFile= "Spaceport, Best date"
        };

        private static readonly LanguageProperties GermanProperties = new LanguageProperties
        {
            FolderNotFound = "Der Ordner existiert nicht. Versuchen Sie es bitte noch einmal!",
            MailSubject = "Bericht über den Start eines Space Shuttles",
            SentSuccessfuly = "E-Mail wurde erfolgreich versendet!",
            SentFailed = "E-Mail könnte nicht gesendet werden.",
            NoDay = "Es gibt keinen perfekten Tag für den Start eines Space Shuttles.",
            NameNotFound = "Fehler: Es gibt keinen Weltraumbahnhof mit diesem Namen! Es gibt keine festgelegte Entfernung zum Äquator.",
            ListEmpty = "Die Liste ist leer!",
            FolderName = "Bitte geben Sie den Ordnernamen ein:",
            SenderEmail = "Bitte geben Sie die E-Mail-Adresse des Absenders ein:",
            Password = "Bitte geben Sie das Passwort ein:",
            ReceiverEmail = "Bitte geben Sie die E-Mail-Adresse des Empfängers ein:",
            InvalidEmail = "Die E-Mail-Adresse ist ungültig!",
            EmailSending = "Die E-Mail wird gesendet...",
            HeaderFile = "Weltraumbahnhof, bestes Datum"
        };
        static void Main()
        {
            string language;
            LanguageProperties properties;
            Console.Write("Welcome on board, Azure Astronaut!\n\n");
            do
            {
                Console.Write("Please select a language option! Type 'E' for English or 'G' for German:");
                language = Console.ReadLine().ToUpper();
                properties = GetLanguageProperties(language);

                if (properties == null)
                {
                    Console.WriteLine("This language option is not valid. Please try again.");
                }
            } while (properties == null);

            string folderNamePath;

            while (true)
            {
                Console.WriteLine(properties.FolderName);  
                
                folderNamePath = Console.ReadLine();
                if (!Directory.Exists(folderNamePath))
                {
                    Console.WriteLine(properties.FolderNotFound);
                }
                else
                {
                    break;
                }
            }
            string senderEmail;
            while (true)
            {
                Console.WriteLine(properties.SenderEmail);
                senderEmail = Console.ReadLine();
                if (!IsEmailValid(senderEmail))
                {
                    Console.WriteLine(properties.InvalidEmail);
                }
                else
                {
                    break;
                }
            }
          
            SecureString password = SecurePassword(properties);

            string receiverEmail;
            while (true)
            {
                Console.WriteLine(properties.ReceiverEmail);
                receiverEmail = Console.ReadLine();
                if (!IsEmailValid(receiverEmail))
                {
                    Console.WriteLine(properties.InvalidEmail);
                }
                else
                {
                    break;
                }
            }
           
           // Console.WriteLine();

            Dictionary<string, List<List<string>>> allData = ReadAllCSVFiles(folderNamePath);
            List<SpaceportInfo> bestLaunchDays = BestLaunchDayPerSpaceport(allData, properties);

            string newFilePath = "LaunchAnalysisReport.csv";

            GenerateReportCSV(newFilePath, bestLaunchDays, properties, language);
            Console.WriteLine();

            string result = BestDateAndLocation(bestLaunchDays, properties, language);
            Console.WriteLine(properties.EmailSending);
            sendEmail(senderEmail, password, receiverEmail,result,properties);
        }
        public static LanguageProperties GetLanguageProperties(string language)
        {
            switch (language)
            {
                case "E":
                    return EnglishProperties;
                case "G":
                    return GermanProperties;
                default:
                    return null;
            }
        }
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                var emailAddr = new MailAddress(email);
                return emailAddr.Address == email;
            }
            catch(FormatException)
            {
                return false;
            }
        }
        public static SecureString SecurePassword(LanguageProperties properties)
        {
            SecureString password = new SecureString();
            ConsoleKeyInfo key;
            Console.WriteLine(properties.Password);
            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (!char.IsControl(key.KeyChar) && key.Key != ConsoleKey.Enter)
                {
                    password.AppendChar(key.KeyChar);
                    Console.Write("*");     
                }              
            }
            Console.WriteLine();
            return password;
        }
        static Dictionary<string, List<List<string>>> ReadAllCSVFiles(string folderNamePath)
        {
            Dictionary<string, List<List<string>>> allData = new Dictionary<string, List<List<string>>>();

            string[] files = Directory.GetFiles(folderNamePath, "*.csv");
            foreach (string file in files)
            {
                List<List<string>> data = ReadCSVFile(file);
                allData.Add(Path.GetFileNameWithoutExtension(file), data);
            }
            return allData;
        }
        static List<List<string>> ReadCSVFile(string filePath)
        {
            List<List<string>> data = new List<List<string>>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    List<string> row = new List<string>(fields);
                    data.Add(row);
                }
            }
            return data;
        }
        static List<SpaceportInfo> BestLaunchDayPerSpaceport(Dictionary<string, List<List<string>>> allData, LanguageProperties properties)
        {
            List<SpaceportInfo> bestLaunchDays = new List<SpaceportInfo>();
            foreach (var fileData in allData)
            {
                List<int> appropriateDays = FindAppropriateDays(fileData.Value);

                SpaceportInfo spaceport = new SpaceportInfo();
                int bestDayPerFile = appropriateDays[0];
                if (bestDayPerFile >= 0)
                {
                    spaceport.Spaceport = fileData.Key;
                    spaceport.BestDay = bestDayPerFile;
                    switch (fileData.Key)
                    {
                        case "Kourou":
                            spaceport.DistanceKm = 500;
                            break;
                        case "Cape Canaveral":
                            spaceport.DistanceKm = 3157;
                            break;
                        case "Kodiak":
                            spaceport.DistanceKm = 6426;
                            break;
                        case "Tanegashima":
                            spaceport.DistanceKm = 3380;
                            break;
                        case "Mahia":
                            spaceport.DistanceKm = 4334;
                            break;
                        default:
                            Console.WriteLine(properties.NameNotFound);
                            break;
                    }
                    bestLaunchDays.Add(spaceport);
                }
                else
                {
                    Console.WriteLine(properties.ListEmpty);
                }
            }
            return bestLaunchDays;
        }
        static List<int> FindAppropriateDays(List<List<string>> data)
        {
            List<int> appropriateDays = new List<int>();
            int size = data.Count;

            for (int i = 1; i < size; i++)
            {
                for (int j = 1; j < data[i].Count; j++)
                {
                    int day;
                    if (int.TryParse(data[0][j], out day))
                    {
                        if (data[i][0] == "Temperature (C)" && day != 0)
                        {
                            if (int.TryParse(data[i][j], out int temperature) && temperature > 1 && temperature < 32)
                            {
                                appropriateDays.Add(day);
                            }
                        }
                        else if (data[i][0] == "Wind (m/s)" && day != 0)
                        {
                            if (int.TryParse(data[i][j], out int windSpeed) && windSpeed > 11)
                            {
                                int index = appropriateDays.IndexOf(day);
                                if (index != -1)
                                {
                                    appropriateDays.RemoveAt(index);
                                }
                            }
                        }
                        else if (data[i][0] == "Humidity (%)" && day != 0)
                        {
                            if (int.TryParse(data[i][j], out int humidityPercent) && humidityPercent >= 55)
                            {
                                int index = appropriateDays.IndexOf(day);

                                if (index != -1)
                                {
                                    appropriateDays.RemoveAt(index);
                                }
                            }
                        }
                        else if (data[i][0] == "Precipitation (%)" && day != 0)
                        {
                            if (int.TryParse(data[i][j], out int percipitationPercent) && percipitationPercent != 0)
                            {
                                int index = appropriateDays.IndexOf(day);

                                if (index != -1)
                                {
                                    appropriateDays.RemoveAt(index);
                                }
                            }
                        }
                        else if (data[i][0] == "Lightning" && day != 0)
                        {
                            if (data[i][j] != "No")
                            {
                                int index = appropriateDays.IndexOf(day);

                                if (index != -1)
                                {
                                    appropriateDays.RemoveAt(index);
                                }
                            }
                        }
                        else if (data[i][0] == "Clouds" && day != 0)
                        {
                            if (data[i][j] == "Cumulus" || data[i][j] == "Nimbus")
                            {
                                int index = appropriateDays.IndexOf(day);

                                if (index != -1)
                                {
                                    appropriateDays.RemoveAt(index);
                                }
                            }
                        }
                    }
                }
                if (!appropriateDays.Any())
                {
                    break;
                }
            }
            if (!appropriateDays.Any())
            {
                appropriateDays.Add(0);
            }
            return appropriateDays;
        }
        public static void GenerateReportCSV(string filePath, List<SpaceportInfo> bestLaunchDays, LanguageProperties properties, string language)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(properties.HeaderFile);

            foreach (var day in bestLaunchDays)
            {
                if (day.BestDay == 0)
                {
                   // Console.WriteLine(day.NoPerfectDayMessage(language));
                    sb.AppendLine(day.NoPerfectDayMessage(language));
                }
                else
                {
                   // Console.WriteLine($"{day.Spaceport}, {day.BestDay}");
                    sb.AppendLine($"{day.Spaceport}, {day.BestDay}");
                }
            }
            File.WriteAllText(filePath, sb.ToString());
        }
        static string BestDateAndLocation(List<SpaceportInfo> bestLaunchDays, LanguageProperties properties, string language)
        {
            string result = "";
            int currentMinDist = 10000;
            foreach (SpaceportInfo obj in bestLaunchDays)
            {
                if (obj.BestDay != 0)
                {
                    currentMinDist = Math.Min(obj.DistanceKm, currentMinDist);
                }
            }
            if (currentMinDist == 10000)
            {
                result = EnglishProperties.NoDay;
            }
            foreach (SpaceportInfo obj in bestLaunchDays)
            {
                if (obj.DistanceKm == currentMinDist)
                {
                    result = obj.ResultMessage(language);
                    //Console.WriteLine(result);
                }
            }
            return result;
        }
        static void sendEmail(string senderEmail, SecureString password, string receiverEmail, string result, LanguageProperties properties)
        {
            try
            {
                MailMessage mail = new MailMessage(senderEmail, receiverEmail);

                mail.Subject = properties.MailSubject;
                mail.Body = $"{result}";

                string attachmentPath = @"LaunchAnalysisReport.csv";
                Attachment fileToAttach = new Attachment(attachmentPath);
                mail.Attachments.Add(fileToAttach);

                SmtpClient smtpClient = new SmtpClient("smtp-mail.outlook.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);

                try
                {
                    smtpClient.Send(mail);
                    Console.WriteLine(properties.SentSuccessfuly);
                }
                catch 
                {
                    Console.WriteLine(properties.SentFailed);
                }
                finally
                {
                    mail.Dispose();
                    smtpClient.Dispose();
                }
            }
            catch
            {
                Console.WriteLine(properties.InvalidEmail);
            }
        }
    }
}