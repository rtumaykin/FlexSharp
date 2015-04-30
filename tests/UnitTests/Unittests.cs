using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rouse.Sales;

namespace UnitTests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void GetCode()
        {
            var cf = new Sample.SampleCodeFactory(null, new CodeFactoryInitializationModel{ ClientId = 1});
            var ssssssc = cf.GetCodeInternal();
        }
    }
}
