using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumberLocation.LocationService
{
    interface IPNLocationSource
    {
        void FullLocation(PNLocationInfo localInfo);
    }
}
