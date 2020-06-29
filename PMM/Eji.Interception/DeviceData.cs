using Device = System.Int32;

namespace Eji.Interception
{
    public class DeviceData
    {

        public Device Device;
        public string Name;

        public DeviceData(Device device, string name)
        {
            Device = device;
            Name = name;
        }

    }
}
