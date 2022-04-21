//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{

    [Flags]
    public enum CanvasFeature
    {
        HasExtensions = 1,
        HasBuffer = 2,
        HardwareAccelerated = 4,
        IsOfficial = 7, // is from the HW vender
        IsCosmos = 15, // is made by the cosmos team
    }

    /// <summary>
    /// Canvas interface.
    /// </summary>
    public interface ICanvas
    {
        /*
         * IReadOnlyList<T> is not working, the Modes inside it become corrupted and then you get Stack Overflow
         */
        //public  IReadOnlyList<Mode> AvailableModes { get; }

        /// <summary>
        /// denotes if this have method that can be used that are not apart of this interface.
        /// </summary>
        public CanvasFeature Features { get; }

        /// <summary>
        /// Available graphics modes.
        /// </summary>
        public List<Mode> AvailableModes { get; }

        /// <summary>
        /// Get default graphics mode.
        /// </summary>
        public Mode DefaultGraphicMode { get; }

        /// <summary>
        /// Get and set graphics mode.
        /// </summary>
        public Mode Mode { get; set; }

        /// <summary>
        /// setup the driver.
        /// </summary>
        public void Init(Mode mode);

        /// <summary>
        /// setup the driver.
        /// </summary>
        public void Init();

        /// <summary>
        /// set the mode.
        /// </summary>
        public void SetMode(Mode mode);

        /// <summary>
        /// Clear all the Canvas with the Black color.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void Clear();

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */
        /// <summary>
        /// Clear all the Canvas with the specified color.
        /// </summary>
        /// <param name="color">Color in ARGB.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void Clear(int color);

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */
        /// <summary>
        /// Clear all the Canvas with the specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void Clear(Color color);

        /// <summary>
        /// Display graphic mode
        /// </summary>
        public void Disable();

        /// <summary>
        /// Draw point.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Point.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawPoint(Pen pen, Point point);

        /// <summary>
        /// Draw point.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawPoint(Pen pen, int x, int y);

        /// <summary>
        /// Draw point to the screen. 
        /// Not implemented.
        /// </summary>
        /// <param name="pen">Pen to draw the point with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always (only int coordinats supported).</exception>
        public void DrawPoint(Pen pen, float x, float y);

        /// <summary>
        /// Name of the backend
        /// </summary>
        public string Name();

        /// <summary>
        /// Display screen
        /// </summary>
        public void Display();

        /// <summary>
        /// Get point color.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Color value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public Color GetPointColor(int x, int y);

        /// <summary>
        /// Draw array of colors.
        /// </summary>
        /// <param name="colors">Colors array.</param>
        /// <param name="point">Starting point.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid, or width is less than 0.</exception>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported.</exception>
        public void DrawArray(Color[] colors, Point point, int width, int height);

        /// <summary>
        /// Draw array of colors.
        /// </summary>
        /// <param name="colors">Colors array.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid, or width is less than 0.</exception>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported.</exception>
        public void DrawArray(Color[] colors, int x, int y, int width, int height);

        /// <summary>
        /// Draw line.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <param name="x2">End point X coordinate.</param>
        /// <param name="y2">End point Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if x1-x2 or y1-y2 equal to Int32.MinValue.</exception>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2);

        /// <summary>
        /// Draw line.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="p1">Staring point.</param>
        /// <param name="p2">End point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if x1-x2 or y1-y2 equal to Int32.MinValue.</exception>
        public void DrawLine(Pen pen, Point p1, Point p2);

        /// <summary>
        /// Draw line.
        /// Not implemented.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <param name="x2">End point X coordinate.</param>
        /// <param name="y2">End point Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2);

        //https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// <summary>
        /// Draw Circle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawCircle(Pen pen, int x_center, int y_center, int radius);

        /// <summary>
        /// Draw Circle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">center point.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawCircle(Pen pen, Point point, int radius);

        /// <summary>
        /// Draw Filled Circle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawFilledCircle(Pen pen, int x0, int y0, int radius);


        /// <summary>
        /// Draw Filled Circle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">center point.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawFilledCircle(Pen pen, Point point, int radius);

        //http://members.chello.at/~easyfilter/bresenham.html
        /// <summary>
        /// Draw ellipse.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="x_radius">X radius.</param>
        /// <param name="y_radius">Y radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawEllipse(Pen pen, int x_center, int y_center, int x_radius, int y_radius);

        /// <summary>
        /// Draw ellipse.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Center point.</param>
        /// <param name="x_radius">X radius.</param>
        /// <param name="y_radius">Y radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawEllipse(Pen pen, Point point, int x_radius, int y_radius);

        /// <summary>
        /// Draw Filled Ellipse.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Center point.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawFilledEllipse(Pen pen, Point point, int height, int width);

        /// <summary>
        /// Draw Filled Ellipse.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x">X Position.</param>
        /// <param name="y">Y Position.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void DrawFilledEllipse(Pen pen, int x, int y, int height, int width);

        /// <summary>
        /// Draw polygon.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="points">Points array.</param>
        /// <exception cref="ArgumentException">Thrown if point array is smaller then 3.</exception>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawPolygon(Pen pen, params Point[] points);

        /// <summary>
        /// Draw square.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Starting point.</param>
        /// <param name="size">size.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawSquare(Pen pen, Point point, int size);

        /// <summary>
        /// Draw square.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="size">size.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawSquare(Pen pen, int x, int y, int size);

        /// <summary>
        /// Draw rectangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Staring point.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawRectangle(Pen pen, Point point, int width, int height);

        /// <summary>
        /// Draw rectangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentNullException">Thrown if pen is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawRectangle(Pen pen, int x, int y, int width, int height);

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point">Starting point.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawFilledRectangle(Pen pen, Point point, int width, int height);

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x_start">Starting point X coordinate.</param>
        /// <param name="y_start">Starting point Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public void DrawFilledRectangle(Pen pen, int x_start, int y_start, int width, int height);

        /// <summary>
        /// Draw triangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="point0">First point.</param>
        /// <param name="point1">Second point.</param>
        /// <param name="point2">Third point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines lengths are invalid.</exception>
        public void DrawTriangle(Pen pen, Point point0, Point point1, Point point2);

        /// <summary>
        /// Draw triangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="v1x">First point X coordinate.</param>
        /// <param name="v1y">First point Y coordinate.</param>
        /// <param name="v2x">Second point X coordinate.</param>
        /// <param name="v2y">Second point Y coordinate.</param>
        /// <param name="v3x">Third point X coordinate.</param>
        /// <param name="v3y">Third point Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if pen is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines lengths are invalid.</exception>
        public void DrawTriangle(Pen pen, int v1x, int v1y, int v2x, int v2y, int v3x, int v3y);

        /// <summary>
        /// Draw rectangle.
        /// Not implemented.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x_start">starting X coordinate.</param>
        /// <param name="y_start">starting Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public void DrawRectangle(Pen pen, float x_start, float y_start, float width, float height);

        //Image and Font will be available in .NET Core 2.1
        // dot net core does not have Image
        //We are using a short term solution for bitmap
        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void DrawImage(Image image, int x, int y);

        /// <summary>
        /// Draw a Scaled Bitmap.
        /// </summary>
        /// <param name="image">Image to Scale.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="w">Desired Width.</param>
        /// <param name="h">Desired Height.</param>
        public void DrawImage(Image image, int x, int y, int w, int h);

        /// <summary>
        /// Draw image with alpha channel.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void DrawImageAlpha(Image image, int x, int y);

        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="point">Point of the top left corner of the image.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void DrawImage(Image image, Point point);

        /// <summary>
        /// Draw image with alpha channel.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="point">Point of the top left corner of the image.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void DrawImageAlpha(Image image, Point point);

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="str">string to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="pen">Color.</param>
        /// <param name="point">Point of the top left corner of the string.</param>
        public void DrawString(string str, Font aFont, Pen pen, Point point);

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="str">string to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="pen">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void DrawString(string str, Font aFont, Pen pen, int x, int y);

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="str">char to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="pen">Color.</param>
        /// <param name="point">Point of the top left corner of the char.</param>
        public void DrawChar(char c, Font aFont, Pen pen, Point point);

        /// <summary>
        /// Draw char.
        /// </summary>
        /// <param name="str">char to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="pen">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void DrawChar(char c, Font aFont, Pen pen, int x, int y);

        /// <summary>
        /// Check if video mode is valid.
        /// </summary>
        /// <param name="mode">Video mode.</param>
        /// <returns>bool value.</returns>
        public bool CheckIfModeIsValid(Mode mode);

        /// <summary>
        /// Check if video mode is valid. Throw exception if not.
        /// </summary>
        /// <param name="mode">Video mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        public void ThrowIfModeIsNotValid(Mode mode);

        /// <summary>
        /// Check if coordinats are valid. Throw exception if not.
        /// </summary>
        /// <param name="point">Point on the convas.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid.</exception>
        public void ThrowIfCoordNotValid(Point point);

        /// <summary>
        /// Check if coordinats are valid. Throw exception if not.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid.</exception>
        public void ThrowIfCoordNotValid(int x, int y);

        /// <summary>
        /// Calculate new Color from back Color with alpha
        /// </summary>
        /// <param name="to">Color to calculate.</param>
        /// <param name="from">Color used to calculate.</param>
        /// <param name="alpha">Alpha amount.</param>
        public Color AlphaBlend(Color to, Color from, byte alpha);

        /// <summary>
        /// returns if the driver is supported on the current system
        /// </summary>
        public bool IsSupported();

    }
}
