using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group1AndroidProject.Models
{
    public static class GeoUtils
    {
        // Method to calculate distance in meters using the Haversine formula
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadius = 6371000; // Earth's radius in meters
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        // Method to calculate direction (bearing) in degrees
        public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
        {
            double dLon = DegreesToRadians(lon2 - lon1);
            double y = Math.Sin(dLon) * Math.Cos(DegreesToRadians(lat2));
            double x = Math.Cos(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) -
                        Math.Sin(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(dLon);

            double bearing = Math.Atan2(y, x);
            return (RadiansToDegrees(bearing) + 360) % 360; // Normalize to 0-360
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
        private static double RadiansToDegrees(double radians) => radians * 180 / Math.PI;
    }
}
