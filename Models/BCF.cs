using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsyncCLI.Models.BCF
{

    //public class BCFToken
    //{
    //    public string access_token { get; set; }
    //    public string token_type { get; set; }
    //}
    public class IssueBoard
    {
        public string project_id { get; set; }
        public string name { get; set; }
        public string bimsync_project_name { get; set; }
        public string bimsync_project_id { get; set; }
    }

    public class IssueBoardExtension
    {
        public List<string> topic_type { get; set; }
        public List<string> topic_status { get; set; }
        public List<string> topic_label { get; set; }
        public List<string> snippet_type { get; set; }
        public List<string> priority { get; set; }
        public List<string> user_id_type { get; set; }
    }

    public class ExtensionStatus
    {
        public string name { get; set; }
        public string color { get; set; }
        public string type { get; set; }
        public bool unlinked { get; set; }
    }

    public class ExtensionType
    {
        public string name { get; set; }
        public string color { get; set; }
        public bool unlinked { get; set; }
    }

    public class ExtensionLabel
    {
        public string name { get; set; }
        public string color { get; set; }
    }

    public class Topic
    {
        public string guid { get; set; }
        public string topic_type { get; set; }
        public string topic_status { get; set; }
        public string title { get; set; }
        public List<string> labels { get; set; }
        public DateTime creation_date { get; set; }
        public string creation_author { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_author { get; set; }
        public DateTime? due_date { get; set; }
        public string assigned_to { get; set; }
        public string description { get; set; }
        public int bimsync_issue_number { get; set; }
        public List<object> reference_links { get; set; }
        public string stage { get; set; }
        public List<object> bimsync_points { get; set; }
        public Assignation bimsync_requester { get; set; }
        public int bimsync_comments_size { get; set; }
        public Assignation bimsync_assigned_to { get; set; }
        public string bimsync_requester_old { get; set; }
    }

    public class Assignation
    {
        public BcfUser user { get; set; }
        public BcfUser team { get; set; }
    }

    public class BcfUser
    {
        public string @ref { get; set; }
        public string email { get; set; }
        public string name { get; set; }
    }

    public class Comment
    {
        public string guid { get; set; }
        public string verbal_status { get; set; }
        public string status { get; set; }
        public DateTime date { get; set; }
        public string author { get; set; }
        public string comment { get; set; }
        public string topic_guid { get; set; }
        public string viewpoint_guid { get; set; }
    }

    public class Viewpoint
    {
        public string guid { get; set; }
        public PerspectiveCamera perspective_camera { get; set; }
        public OrthogonalCamera orthogonal_camera { get; set; }
        public List<Line> lines { get; set; }
        public List<ClippingPlane> clipping_planes { get; set; }
    }

    public class Vector
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        /// <summary>
        /// Create a null Vector
        /// </summary>
        public Vector()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        /// <summary>
        /// Create a vector from x,y and z coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the vector.</param>
        /// <param name="y">Thy y coordinate of the vector.</param>
        /// /// <param name="y">Thy y coordinate of the vector.</param>
        /// <exception>Thrown if any components of the vector are NaN or Infinity.</exception>
        public Vector(double x, double y, double z)
        {
            if (Double.IsNaN(x) || Double.IsNaN(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was NaN.");
            }

            if (Double.IsInfinity(x) || Double.IsInfinity(y))
            {
                throw new ArgumentOutOfRangeException("The vector could not be created. One or more of the components was infinity.");
            }

            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return string.Format("{{({0},{1},{2})}}", x.ToString(), y.ToString(), z.ToString());
        }

        /// <summary>
        /// The angle in degrees from this vector to the provided vector.
        /// </summary>
        /// <param name="v">The vector with which to measure the angle.</param>
        public double AngleTo(Vector v)
        {
            var rad = Math.Acos((Dot(v) / (Length() * v.Length())));
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Compute the dot product of this vector and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the dot product.</param>
        /// <returns>The dot product.</returns>
        public double Dot(Vector v)
        {
            return v.x * this.x + v.y * this.y + v.z * this.z;
        }

        /// <summary>
        /// Get the length of this vector.
        /// </summary>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        /// <summary>
        /// Multiply a vector and a scalar.
        /// </summary>
        /// <param name="v">The vector to multiply.</param>
        /// <param name="a">The scalar value to multiply.</param>
        /// <returns>A vector whose magnitude is multiplied by a.</returns>
        public static Vector operator *(Vector v, double a)
        {
            return new Vector(v.x * a, v.y * a, v.z * a);
        }

        /// <summary>
        /// Multiply a scalar and a vector.
        /// </summary>
        /// <param name="a">The scalar value to multiply.</param>
        /// <param name="v">The vector to multiply.</param>
        /// <returns>A vector whose magnitude is mutiplied by a.</returns>
        public static Vector operator *(double a, Vector v)
        {
            return new Vector(v.x * a, v.y * a, v.z * a);
        }

        /// <summary>
        /// Divide a vector by a scalar.
        /// </summary>
        /// <param name="a">The scalar divisor.</param>
        /// <param name="v">The vector to divide.</param>
        /// <returns>A vector whose magnitude is mutiplied by a.</returns>
        public static Vector operator /(Vector v, double a)
        {
            return new Vector(v.x / a, v.y / a, v.z / a);
        }

        /// <summary>
        /// Subtract two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A vector which is the difference between a and b.</returns>
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector((a.x - b.x), (a.y - b.y), (a.z - b.z));
        }

        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>A vector which is the sum of a and b.</returns>
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector((a.x + b.x), (a.y + b.y), (a.z + b.z));
        }

        /// <summary>
        /// Return a new vector which is the unitized version of this vector.
        /// </summary>
        public Vector Normalise()
        {
            var length = Length();
            if (length == 0)
            {
                return this;
            }
            return new Vector(x / length, y / length, z / length);
        }

        /// <summary>
        /// Construct a new vector which is the inverse of this vector.
        /// </summary>
        /// <returns>A new vector which is the inverse of this vector.</returns>
        public Vector Negate()
        {
            return new Vector(-x, -y, -z);
        }

        /// <summary>
        /// Project a vector on the XY plane.
        /// </summary>
        /// <returns>A new vector which is projected on the XY plane.</returns>
        public Vector Project()
        {
            return new Vector(this.x, this.y, 0);
        }

        /// <summary>
        /// Compute the cross product of this vector and v.
        /// </summary>
        /// <param name="v">The vector with which to compute the cross product.</param>
        public Vector Cross(Vector v)
        {
            var x = this.y * v.z - this.z * v.y;
            var y = this.z * v.x - this.x * v.z;
            var z = this.x * v.y - this.y * v.x;

            return new Vector(x, y, z);
        }
    }

        public class Camera
    {
        public Vector camera_view_point { get; set; }
        public Vector camera_direction { get; set; }
        public Vector camera_up_vector { get; set; }
        public double field { get; set; }
    }

    public class PerspectiveCamera
    {
        public Vector camera_view_point { get; set; }
        public Vector camera_direction { get; set; }
        public Vector camera_up_vector { get; set; }
        public double field_of_view { get; set; }

        public Camera GetCamera()
        {
            Camera camera = new Camera();
            camera.camera_direction = camera_direction;
            if (camera_up_vector.Length() == 0)
            {
                Vector normalVector = camera_direction.Cross(new Vector(0, 0, 1));
                camera.camera_up_vector = normalVector.Cross(camera_direction);
            }
            else
            {
                camera.camera_up_vector = camera_up_vector;
            }
            
            camera.camera_view_point = camera_view_point;
            camera.field = field_of_view;
            return camera;
        }
    }

    public class OrthogonalCamera
    {
        public Vector camera_view_point { get; set; }
        public Vector camera_direction { get; set; }
        public Vector camera_up_vector { get; set; }
        public double view_to_world_scale { get; set; }

        public Camera GetCamera()
        {
            Camera camera = new Camera();
            camera.camera_direction = camera_direction;

            if (camera_up_vector.Length() == 0)
            {
                Vector normalVector = camera_direction.Cross(new Vector(0, 0, 1));
                camera.camera_up_vector = normalVector.Cross(camera_direction);
            }
            else
            {
                camera.camera_up_vector = camera_up_vector;
            }
            camera.camera_view_point = camera_view_point;
            camera.field = view_to_world_scale;
            return camera;
        }
    }

    public class Line
    {
        public Vector start_point { get; set; }
        public Vector end_point { get; set; }
    }

    public class ClippingPlane
    {
        public Vector location { get; set; }
        public Vector direction { get; set; }
    }

    public class IfcObject
    {
        public IfcObject(string guid)
        {
            ifcGuid = guid;
        }
        public IfcObject()
        {
        }

        public string ifcGuid { get; set; }
    }
}
