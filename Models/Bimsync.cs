using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsyncCLI.Models.Bimsync
{
    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string imageUrl { get; set; }
        public User owner { get; set; }
        public SiteLocation siteLocation { get; set; }
    }

    public class SiteLocation
    {
        public double? longitude { get; set; }
        public double? latitude { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Member
    {
        public string role { get; set; }
        public string visibility { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public string createdAt { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string avatarUrl { get; set; }
        public string type { get; set; }
    }

    public class Revision
    {
        public string comment { get; set; }
        public string createdAt { get; set; }
        public string id { get; set; }
        public Model model { get; set; }
        public User user { get; set; }
        public int version { get; set; }
    }

    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }


    public class RevisionStatus
    {
        public string callbackUri { get; set; }
       public string filename { get; set; }
        public string id { get; set; }
        public ProcessingStatus processing { get; set; }
        public Model model { get; set; }
        public Revision revision { get; set; }
        public User user { get; set; }
    }

    public class ProcessingStatus
    {
        public double? progress { get; set; }
        public string status { get; set; }
        public Error error { get; set; }
    }

    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class Transform : IEquatable<Transform>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Angle { get; set; }

        public static Transform Identity()
        {
            return new Transform
            {
                X = 0,
                Y = 0,
                Z = 0,
                Angle = 0
            };
        }

        public static Transform Copy(Transform transform)
        {
            return new Transform
            {
                X = transform.X,
                Y = transform.Y,
                Z = transform.Z,
                Angle = transform.Angle
            };
        }

        public override bool Equals(Object obj)
        {
            Transform other = obj as Transform;
            if (other == null) return false;

            return Equals(other);
        }

        public bool Equals(Transform other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (X == other.X &&
                    Y == other.Y &&
                    Z == other.Z &&
                    Angle == other.Angle);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + this.X.GetHashCode();
            hash = hash * 23 + this.Y.GetHashCode();
            hash = hash * 23 + this.Z.GetHashCode();
            hash = hash * 23 + this.Angle.GetHashCode();

            return hash;
        }
    }
}

