using System;
using System.IO;

namespace Businesscards.Models
{
    public class User
    {
        private static readonly User instanceUser = new User();
        private string originUser;

        private User()
        {

        }

        public static User InstanceUser
        {
            get
            {
                return instanceUser;
            }
        }


        public string OriginUser
        {
            get { return originUser; }
            set
            {
                originUser = value;
            }
        }

        // Used to store the origin as a file
        string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "origin.txt");

        public void setOriginWithTxt()
        {
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            string fileContents = originUser;

            File.WriteAllText(localPath, fileContents);

        }

        public string getOriginWithTxt()
        {
            if (File.Exists(localPath))
            {
                return File.ReadAllText(localPath);
            }
            return null;
        }

    }
}
