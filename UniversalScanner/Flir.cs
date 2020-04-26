using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UniversalScanner
{
    class Flir : ScanEngine
    {
        protected mDNS dnsBroker;
        private static readonly string domain = "_flir-ircam._tcp.local" ;

        public override int color
        {
            get
            {
                return 0x806000;
            }
        }
        public override string name
        {
            get
            {
                return "Flir";
            }
        }

        public Flir()
        {
            dnsBroker = mDNS.getInstance();

            dnsBroker.registerDomain(domain, flirDeviceFound);
        }

        public override void reciever(IPEndPoint from, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void scan()
        {
#if DEBUG
            //dnsBroker.selfTest("Flir.selftest");
#endif
            dnsBroker.scan(domain, mDNSType.TYPE_PTR);
        }

        public override byte[] sender(IPEndPoint dest)
        {
            return (new byte[0]);
        }

        public void flirDeviceFound(string domainFilter, mDNSAnswer[] answers)
        {
            IPAddress ip;
            string deviceModel, serial;

            ip = null;
            deviceModel = null;
            serial = null;
            foreach (var a in answers)
            {
                switch (a.Type)
                {
                    case mDNSType.TYPE_A:
                        if (ip == null)
                        {
                            ip = a.data.typeA;
                        }
                        break;
                    case mDNSType.TYPE_AAAA:
                        break;
                    case mDNSType.TYPE_ANY:
                        break;
                    case mDNSType.TYPE_PTR:
                        if (deviceModel == null)
                        {
                            deviceModel = a.data.typePTR;
                            if (deviceModel.Contains('.'))
                            {
                                deviceModel = deviceModel.Split('.')[0];
                            }
                        }
                        break;
                    case mDNSType.TYPE_SRV:
                        break;
                    case mDNSType.TYPE_TXT:
                        // do we have a TXT with serial in Flir
                        if (serial == null)
                        {
                            serial = a.data.typeTXT[0];
                            if (serial.Contains('='))
                            {
                                serial = serial.Split('=')[1];
                            }
                        }
                        break;
                }
            }

            if (ip != null)
            {
                if (deviceModel == null) deviceModel = "unknown";

                /*
                 * Axis way, probably different with Flir:
                 *
                int modelMacSplitter = deviceModel.IndexOf(" - ");
                if (modelMacSplitter >= 0)
                {
                    if (serial == null)
                    {
                        serial = deviceModel.Substring(modelMacSplitter + 3);
                    }
                    deviceModel = deviceModel.Substring(0, modelMacSplitter);
                }
                */

                if (serial == null) serial = "unknown";
                viewer.deviceFound(name, 1, ip.ToString(), deviceModel, serial);
            }
        }
    }
}
