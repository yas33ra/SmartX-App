//combining electrical meter readings to demonstrate operator overloading
namespace SmartX.API.Models
{
    public class PowerReading
    {
        public int Watts { get; set; }

        public static PowerReading operator +(PowerReading a, PowerReading b)
        {
            return new PowerReading { Watts = a.Watts + b.Watts };
        }

//---------------------------------------------------------------------------------


        public static bool operator >(PowerReading a, PowerReading b)
        {
            return a.Watts > b.Watts;
        }

        public static bool operator <(PowerReading a, PowerReading b)
        {
            return a.Watts < b.Watts;
        }
    }

}
