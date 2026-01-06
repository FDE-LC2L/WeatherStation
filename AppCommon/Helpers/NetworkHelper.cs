using System.Net;
using System.Net.NetworkInformation;

namespace AppCommon.Helpers
{
    public static class NetworkHelper
    {

        /// <summary>
        /// Represents detailed information about a network interface, including its name, description, IP address, speed, type, and a unique identifier.
        /// </summary>
        public class NetworkInterfaceInfo
        {
            public required string Name { get; set; } = string.Empty;
            public required string Description { get; set; } = string.Empty;
            public required IPAddress IPAddressV4 { get; set; }
            public long Speed { get; set; }
            public NetworkInterfaceType Type { get; set; }
            public required string Id { get; set; } = string.Empty;
            public string FormattedSpeed { get => FormatNetworkSpeed(Speed); }
        }

        /// <summary>
        /// Retrieves a list of network interfaces that are operational, non-loopback, and have an IPv4 address.
        /// </summary>
        /// <returns>
        /// A list of <see cref="NetworkInterfaceInfo"/> objects, each representing a network interface
        /// with its name, description, IP address, speed, type, and a unique identifier.
        /// </returns>
        public static List<NetworkInterfaceInfo> GetPotentialNetworkInterfaces()
        {
            var result = new List<NetworkInterfaceInfo>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.Speed > 0 &&
                    ni.GetIPProperties().UnicastAddresses.Any(addr =>
                        addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                );

            foreach (var ni in interfaces)
            {
                var ipv4 = ni.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address;

                if (ipv4 != null)
                {
                    // Créer un ID unique basé sur des propriétés stables de l'interface
                    string interfaceId = $"{ni.Id}_{ipv4}";

                    result.Add(new NetworkInterfaceInfo
                    {
                        Name = ni.Name,
                        Description = ni.Description,
                        IPAddressV4 = ipv4,
                        Speed = ni.Speed,
                        Type = ni.NetworkInterfaceType,
                        Id = interfaceId
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Formats a network speed value (in bits per second) into a human-readable string.
        /// </summary>
        /// <param name="speedInBitsPerSecond">The network speed in bits per second.</param>
        /// <returns>
        /// A string representing the formatted network speed, using "Gb" for gigabits, 
        /// "Mb" for megabits, or "bps" for speeds below 1 megabit.
        /// </returns>
        public static string FormatNetworkSpeed(long speedInBitsPerSecond)
        {
            const long OneGigabit = 1_000_000_000; // 1 Gb in bits
            const long OneMegabit = 1_000_000;     // 1 Mb in bits

            if (speedInBitsPerSecond >= OneGigabit)
            {
                // Convert to gigabits and return the formatted string
                return $"{speedInBitsPerSecond / OneGigabit} Gb";
            }
            else if (speedInBitsPerSecond >= OneMegabit)
            {
                // Convert to megabits and return the formatted string
                return $"{speedInBitsPerSecond / OneMegabit} Mb";
            }
            else
            {
                // Return the speed in bits per second for values below 1 Mb
                return $"{speedInBitsPerSecond} bps";
            }
        }


    }


}
