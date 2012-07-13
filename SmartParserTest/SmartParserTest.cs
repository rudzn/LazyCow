using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LazyCow;

namespace SmartParserTest
{
    [TestClass]
    public class SmartParserTest
    {
        [TestMethod]
        public void MakeInstance()
        {
            var parser = new SmartParser();
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void AllMembersInitialized()
        {
            var parser = new SmartParser();

            foreach (var properties in parser.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance))
            {
                var property = parser.GetType().GetProperty(properties.Name);
                Assert.IsNotNull(property.GetValue(parser, null));
            }

            foreach (var field in parser.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.IsNotNull(field.GetValue(parser));
            }
        }

        [TestMethod]
        public void SetGetText()
        {
            var parser = new SmartParser();
            try
            {
                parser.Text = "Test";
            }
            catch
            {
                Assert.Fail();
            }
            Assert.IsNotNull(parser.Text);
        }

        [TestMethod]
        public void ParseShortNumericDate()
        {
            var parser = new SmartParser {Text = "^11.11.11"};
            var date = new DateTime(2011, 11, 11);

            Assert.IsTrue(date.Year     == parser.Date.Year);
            Assert.IsTrue(date.Month    == parser.Date.Month);
            Assert.IsTrue(date.Day      == parser.Date.Month);

            parser = new SmartParser { Text = "11.11.11" };

            Assert.IsFalse(date.Year == parser.Date.Year);
            Assert.IsFalse(date.Month == parser.Date.Month);
            Assert.IsFalse(date.Day == parser.Date.Month);
        }

        [TestMethod]
        public void ParseLongNumericDate()
        {
            var parser = new SmartParser { Text = "^11.11.2011" };
            var date = new DateTime(2011, 11, 11);

            Assert.IsTrue(date.Year == parser.Date.Year);
            Assert.IsTrue(date.Month == parser.Date.Month);
            Assert.IsTrue(date.Day == parser.Date.Month);

            parser = new SmartParser { Text = "11.11.2011" };

            Assert.IsFalse(date.Year == parser.Date.Year);
            Assert.IsFalse(date.Month == parser.Date.Month);
            Assert.IsFalse(date.Day == parser.Date.Month);
        }

        [TestMethod]
        public void ParseTime()
        {
            var parser = new SmartParser {Text = "^11.11.11 11:11"};
            var date = new DateTime(2011, 11, 11, 11, 11, 0);

            Assert.IsTrue(date.Hour     == parser.Date.Hour);
            Assert.IsTrue(date.Minute   == parser.Date.Minute);

            parser.Text = "^11:11";

            Assert.IsFalse(date.Hour == parser.Date.Hour);
            Assert.IsFalse(date.Minute == parser.Date.Minute);

            parser.Text = "^11.11.11 23:59";

            Assert.IsTrue(parser.Date.Hour == 23);
            Assert.IsTrue(parser.Date.Minute == 59);
        }

        [TestMethod]
        public void ParseList()
        {
            var parser = new SmartParser {Text = "#persönlich"};

            Assert.IsTrue(System.String.CompareOrdinal(parser.List, "persönlich") == 0);
        }

        [TestMethod]
        public void ParseTags()
        {
            var parser = new SmartParser {Text = "#doesntmatter #tag1 #tag2"};

            Assert.IsTrue(parser.Tags.Any());
            Assert.IsTrue(parser.Tags.Count == 2);
            Assert.IsTrue(System.String.CompareOrdinal(parser.Tags[0], "tag1") == 0);
            Assert.IsTrue(System.String.CompareOrdinal(parser.Tags[1], "tag2") == 0);
        }

        [TestMethod]
        public void ParseLocation()
        {
            var parser = new SmartParser {Text = "@home"};

            Assert.IsTrue(System.String.CompareOrdinal(parser.Location, "home") == 0);
        }

        [TestMethod]
        public void ParseSubject()
        {
            var parser = new SmartParser {Text = "#whatever subject ^11.11.11"};
            Assert.IsTrue(string.CompareOrdinal(parser.Subject, "subject") == 0);

            parser.Text = "subject #whocares @thisplace";
            Assert.IsTrue(string.CompareOrdinal(parser.Subject, "subject") == 0);
        }

        [TestMethod]
        public void ParseSampleTexts()
        {
            var samples = new List<string>()
                              {
                                  "test #persönlich @home #LazyCow ^11.11.11 11:11",
                                  "#persönlich @home #LazyCow ^11.11.2011 11:11 test"
                              };

            var date = new DateTime(2011, 11, 11, 11, 11, 0);

            var parser = new SmartParser();

            foreach (var sample in samples)
            {
                parser.Text = sample;

                Assert.IsTrue(parser.Date.Day == date.Day);
                Assert.IsTrue(parser.Date.Month == date.Month);
                Assert.IsTrue(parser.Date.Year == date.Year);
                Assert.IsTrue(parser.Date.Hour == date.Hour);
                Assert.IsTrue(parser.Date.Minute == date.Minute);
                Assert.IsTrue(string.CompareOrdinal(parser.List, "persönlich") == 0);
                Assert.IsTrue(string.CompareOrdinal(parser.Tags[0], "LazyCow") == 0);
                Assert.IsTrue(string.CompareOrdinal(parser.Location, "home") == 0);
            }
        }
    }
}
