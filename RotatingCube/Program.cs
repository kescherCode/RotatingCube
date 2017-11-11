using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace RotatingCube
{
    class Cubes
    {
        static IEnumerable<CornerData> lines;
        static ConsoleKeyInfo keyPress;
        static bool altDown, shiftDown;
        class CornerData
        {
            public Point3D a;
            public Point3D b;
            public CornerData(Point3D a, Point3D b)
            {
                this.a = a;
                this.b = b;
            }
        }
        static void Init()
        {
            Console.CursorVisible = false;
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
            Console.BufferHeight = Console.WindowHeight = 30;
            Console.BufferWidth = Console.WindowWidth = 60;
            Console.Title = "Press ESC to exit | Rotating Cube Demo by Jeremy Kescher";
        }
        static void Print2DProjection(float angX, float angY, float angZ)
        {
            foreach (var line in lines)
            {
                for (int i = 0; i < 25; i++)
                {
                    // Find a point between A and B by following formula p=a+z(b-a) where z
                    // is a value between 0 and 1.
                    var point = line.a + (i * 1.0f / 24) * (line.b - line.a);
                    // Rotates the point relative to all the angles.
                    Point3D r = point.RotateX(angX).RotateY(angY).RotateZ(angZ);
                    // Projects the point into 2d space. Acts as a kind of camera setting.
                    Point3D q = r.Project(40, 40, 300, 4);
                    // Setting the cursor to a projecting position
                    Console.SetCursorPosition(((int)q.x + 265) / 10, ((int)q.y + 135) / 10);
                    Console.Write('█');
                }
            }
        }
        static void Main()
        {
            Init();

            // Simply a list of all possible corners of a cube in 3D space
            List<Point3D> corners = new List<Point3D>
            {
                new Point3D(-1, -1, -1),
                new Point3D(1, -1, -1),
                new Point3D(1, -1, 1),
                new Point3D(-1, -1, 1),
                new Point3D(-1, 1, 1),
                new Point3D(-1, 1, -1),
                new Point3D(1, 1, -1),
                new Point3D(1, 1, 1)
            };

            // A LINQ query getting all corners for 2D space, returning them to a class-wide struct
            lines = from a in corners
                    from b in corners
                    where (a - b).Length == 2 && a.x + a.y + a.z > b.x + b.y + b.z
                    select new CornerData(a, b);

            // Starting angle
            float angX = 0f, angY = 0f, angZ = 0f;
            // If escape is pressed later, the program will exit
            Random randFactor = new Random();
            bool exit = false;
            bool manualControl = true;
            float rotationFactor = 1f;
            while (!exit)
            {
                Print2DProjection(angX, angY, angZ);
                if (manualControl)
                {
                    keyPress = Console.ReadKey(true);
                    altDown = (keyPress.Modifiers & ConsoleModifiers.Alt) != 0;
                    shiftDown = (keyPress.Modifiers & ConsoleModifiers.Shift) != 0;
                    if (shiftDown)
                    {
                        if (altDown) rotationFactor = 1f;
                        else rotationFactor = 0.5f;
                    }
                    else if (altDown) rotationFactor = 2f;
                    else rotationFactor = 1f;
                    switch (keyPress.Key)
                    {
                        case ConsoleKey.W:
                            angX += rotationFactor;
                            break;
                        case ConsoleKey.A:
                            angY += rotationFactor;
                            break;
                        case ConsoleKey.S:
                            angX -= rotationFactor;
                            break;
                        case ConsoleKey.D:
                            angY -= rotationFactor;
                            break;
                        case ConsoleKey.J:
                            angZ += rotationFactor;
                            break;
                        case ConsoleKey.K:
                            angZ -= rotationFactor;
                            break;
                        case ConsoleKey.R:
                            angX = 0f; angY = 0f; angZ = 0f;
                            break;
                        case ConsoleKey.M:
                            manualControl = false;
                            break;
                        case ConsoleKey.Escape:
                            exit = true;
                            break;
                    }
                }
                else
                {
                    if (Console.KeyAvailable)
                    {
                        keyPress = Console.ReadKey(true);
                        switch (keyPress.Key)
                        {
                            case ConsoleKey.M:
                                manualControl = true;
                                break;
                            case ConsoleKey.Escape:
                                exit = true;
                                break;
                        }
                    }
                    angX += randFactor.Next(1, 2);
                    angY += randFactor.Next(1, 2);
                    angZ += randFactor.Next(1, 2);
                    System.Threading.Thread.Sleep(50);
                }
                Console.Clear();
            }
        }
    }


    class Point3D
    {
        public float x;
        public float y;
        public float z;

        public Point3D(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public Point3D RotateX(float angle)
        {
            float rad = angle * (float)Math.PI / 180;
            float cosa = (float)Math.Cos(rad);
            float sina = (float)Math.Sin(rad);
            float tempY = y;
            y = y * cosa - z * sina;
            z = tempY * sina + z * cosa;
            return this;
        }

        public Point3D RotateY(float angle)
        {
            float rad = angle * (float)Math.PI / 180;
            float cosa = (float)Math.Cos(rad);
            float sina = (float)Math.Sin(rad);
            float tempX = x;
            x = z * sina + x * cosa;
            z = z * cosa - tempX * sina;
            return this;
        }

        public Point3D RotateZ(float angle)
        {
            float rad = angle * (float)Math.PI / 180;
            float cosa = (float)Math.Cos(rad);
            float sina = (float)Math.Sin(rad);
            float tempX = x;
            x = x * cosa - y * sina;
            y = y * cosa + tempX * sina;
            return this;
        }

        public Point3D Project(float width, float height, float fov, float viewDist)
        {
            float factor = fov / (viewDist + z);
            Point3D p = new Point3D(x, y, z);
            return new Point3D(p.x * factor + width / 2, -p.y * factor + height / 2, 1);
        }

        public float Length { get { return (float)Math.Sqrt(x * x + y * y + z * z); } }
        public static Point3D operator *(float scale, Point3D x)
        {
            Point3D p = new Point3D(x.x, x.y, x.z);
            p.x *= scale;
            p.y *= scale;
            p.z *= scale;
            return p;
        }

        public static Point3D operator -(Point3D left, Point3D right)
        {
            Point3D p = new Point3D(left.x, left.y, left.z);
            p.x -= right.x;
            p.y -= right.y;
            p.z -= right.z;
            return p;
        }

        public static Point3D operator +(Point3D left, Point3D right)
        {
            Point3D p = new Point3D(left.x, left.y, left.z);
            p.x += right.x;
            p.y += right.y;
            p.z += right.z;
            return p;
        }
    }
}