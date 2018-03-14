using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvUtility;

namespace CsvUtilityTest
{
    [TestClass]
    public class CsvWriterTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string filePath = string.Format(@"{0}\test.csv", AppDomain.CurrentDomain.BaseDirectory);
            var writer = new CsvWriter(filePath);
            string[] values = new string[] { "11", "22,2", "3,3" };
            writer.WriteRecord(values);
            writer.Close();
        }
    }
}
