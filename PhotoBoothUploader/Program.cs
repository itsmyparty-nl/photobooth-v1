using System;
using System.IO;
using System.Linq;
using ItsMyParty.Photobooth.Api;
using ItsMyParty.Photobooth.Client;

namespace PhotoUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Provide a valid folder as argument!");
                return;
            }
            var eventFolder = args[0];
            if (!Directory.Exists(eventFolder))
            {
                Console.WriteLine("Provided folder does not exist: "+ eventFolder);
                return;
            }
            //var apiUri = "https://photobooth.itsmyparty.nl";
            var apiUri = "http://localhost:50353";
            var eventApi = new EventApiApi(apiUri);
            var sessionApi = new SessionApiApi(apiUri);
            var shotApi = new ShotApiApi(apiUri);

            int eventId = 1440;

            foreach (var subFolder in Directory.EnumerateDirectories(eventFolder).OrderBy(f => f))
            {
                try
                {
                    var index = Convert.ToInt32(subFolder.Replace(eventFolder + @"\", ""));
                    var timestamp = Directory.GetCreationTime(subFolder);
                    var session = new SessionDTO() {Index = index, Timestamp = timestamp, EventId = eventId};
                    var createdSession = sessionApi.CreateEventSession(eventId, session);
                    Console.WriteLine("Sweeping folder: " + subFolder);
                    foreach (var file in Directory.GetFiles(subFolder, "*.jpg"))
                    {
                        try
                        {
                            var fullFilename = Path.Combine(subFolder, file);
                            Console.WriteLine("Adding file: "+fullFilename);
                            var isCollage = file.Contains("collage");
                            var shot = new ShotDTO() {SessionId = createdSession.Id, IsCollage = isCollage};

                            using (FileStream stream = new FileStream(fullFilename, FileMode.Open))
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    shot.Image = Base64ImageConverter.ToBase64Jpg(ms.ToArray());
                                }
                            }

                            shotApi.CreateEventSessionShot(eventId, createdSession.Index, shot);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error while adding file:" + file + ". Exception: " + e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("skipping subfolder: "+subFolder+". Exception: "+e.Message);
                }
            }
        }
    }
}
