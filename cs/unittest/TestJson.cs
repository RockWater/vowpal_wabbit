﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VW;
using VW.Labels;
using VW.Serializer;
using VW.Serializer.Attributes;
using VW.Serializer.Intermediate;

namespace cs_unittest
{
    [TestClass]
    public class TestJsonClass
    {
        [TestMethod]
        [TestCategory("JSON")]
        public void TestJson()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("|a foo:1", "{\"a\":{\"foo\":1}}");
                validator.Validate("|a foo:2.3", "{\"a\":{\"foo\":2.3}}");
                validator.Validate("|a foo:2.3 bar", "{\"a\":{\"foo\":2.3, \"bar\":true}}");
                validator.Validate("|a foo:1 |bcd Age25_old", "{\"a\":{\"foo\":1},\"bcd\":{\"Age\":\"25 old\"}}");
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonAux()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("|a foo:1", "{\"a\":{\"foo\":1},\"_aux\":{\"abc\":{\"def\":3}}}");
                validator.Validate("|a foo:1", "{\"a\":{\"foo\":1},\"_aux\":5}");
                validator.Validate("|a foo:1", "{\"a\":{\"foo\":1},\"_aux\":[1,2,[3,4],2]}");
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonArray()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("| :1 :2.3 :4", "{\"a\":[1,2.3,4]}");
                validator.Validate("|a :1 :2.3 :4", "{\"a\":{\"b\":[1,2.3,4]}}");
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonSimpleLabel()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("1 |a foo:1", "{\"_label\":{\"Label\":1},\"a\":{\"foo\":1}}", VowpalWabbitLabelComparator.Simple);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonVWLabel()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("1 |a foo:1", "{\"_label\":1,\"a\":{\"foo\":1}}", VowpalWabbitLabelComparator.Simple);
                validator.Validate("1 |a foo:1", "{\"_label\":\"1\",\"a\":{\"foo\":1}}", VowpalWabbitLabelComparator.Simple);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonSimpleLabelOverride()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator())
            {
                validator.Validate("2 |a foo:1", "{\"_label\":{\"Label\":1},\"a\":{\"foo\":1}}", VowpalWabbitLabelComparator.Simple,
                    new SimpleLabel { Label = 2 });
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonContextualBanditLabel()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator("--cb 2 --cb_type dr"))
            {
                //validator.Validate("1:-2:.3 |a foo:1",
                //    "{\"_label\":{\"Action\":1,\"Cost\":-2,\"Probability\":.3},\"a\":{\"foo\":1}}",
                //    VowpalWabbitLabelComparator.ContextualBandit);
                validator.Validate("1:2:.5 |a foo:1", "{\"_label\":\"1:2:.5\",\"a\":{\"foo\":1}}", VowpalWabbitLabelComparator.ContextualBandit);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonToVWString()
        {
            var jsonContext = new JsonContext()
            {
                Label = new SimpleLabel
                {
                    Label = 25
                },
                Ns1 = new Namespace1
                {
                    Foo = 1,
                    Age = "25",
                    DontConsider = "XXX"
                },
                Ns2 = new Namespace2
                {
                    FeatureA = true
                },
                Clicks = 5
            };

            var jsonContextString = JsonConvert.SerializeObject(jsonContext);
            using (var validator = new VowpalWabbitExampleJsonValidator(""))
            {
                validator.Validate("25 |a Bar:1 Age25 |b Marker | Clicks:5 MoreClicks:0",
                    jsonContextString,
                    VowpalWabbitLabelComparator.Simple);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonMultiline()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator("--cb 2 --cb_type dr"))
            {
                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     " | w2 w3"
                    },
                    "{\"Age\":25,\"_multi\":[{\"_text\":\"w1 w2\", \"a\":{\"x\":1}}, {\"_text\":\"w2 w3\"}]}");

                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     "2:-1:.3 | w2 w3"
                    },
                    "{\"Age\":25,\"_multi\":[{\"_text\":\"w1 w2\", \"a\":{\"x\":1}}, {\"_text\":\"w2 w3\",\"_label\":\"2:-1:.3\"}]}",
                    VowpalWabbitLabelComparator.ContextualBandit);
            }

            using (var validator = new VowpalWabbitExampleJsonValidator(
                new VowpalWabbitSettings
                {
                    Arguments = "--cb 2 --cb_type dr",
                    PropertyConfiguration = new PropertyConfiguration
                    {
                        MultiProperty = "adf",
                        TextProperty = "someText",
                        LabelProperty = "theLabel",
                        FeatureIgnorePrefix = "xxx"
                    }
                }))
            {
                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     "2:-1:.3 | w2 w3"
                    },
                    "{\"Age\":25,\"adf\":[{\"someText\":\"w1 w2\", \"a\":{\"x\":1}, \"xxxxIgnoreMe\":2}, {\"someText\":\"w2 w3\",\"theLabel\":\"2:-1:.3\"}]}",
                    VowpalWabbitLabelComparator.ContextualBandit);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonText()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator(""))
            {
                validator.Validate("| a b c |a d e f", "{\"_text\":\"a b c\",\"a\":{\"_text\":\"d e f\"}}");
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonNumADFs()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator(""))
            {
                Assert.AreEqual(2,
                    VowpalWabbitJsonSerializer.GetNumberOfActionDependentExamples(
                    "{\"_text\":\"a b c\",\"a\":{\"_text\":\"d e f\"},_multi:[{\"a\":1},{\"b\":2,\"c\":3}]}"));

                Assert.AreEqual(0,
                    VowpalWabbitJsonSerializer.GetNumberOfActionDependentExamples(
                    "{\"_text\":\"a b c\",\"a\":{\"_text\":\"d e f\"},_multi:[]}"));

                Assert.AreEqual(0,
                    VowpalWabbitJsonSerializer.GetNumberOfActionDependentExamples(
                    "{\"_text\":\"a b c\",\"a\":{\"_text\":\"d e f\"}}"));
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonLabel()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator(""))
            {
                validator.Validate("1 | a:2 ", "{\"a\":2,\"_label_Label\":1}");
            }

            using (var validator = new VowpalWabbitExampleJsonValidator(new VowpalWabbitSettings
                {
                    Arguments = "--cb_adf",
                    PropertyConfiguration = new PropertyConfiguration
                    {
                        MultiProperty = "adf",
                        TextProperty = "someText",
                        FeatureIgnorePrefix = "xxx"
                    }
                }))
            {
                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     "0:-1:.3 | w2 w3"
                    },
                    "{\"Age\":25,\"adf\":[{\"someText\":\"w1 w2\", \"a\":{\"x\":1}, \"xxxxIgnoreMe\":2}, {\"someText\":\"w2 w3\"}], \"_labelIndex\":1, \"_label_Cost\":-1, \"_label_Probability\":0.3}",
                    VowpalWabbitLabelComparator.ContextualBandit);

                // all lower case (ASA issue)
                validator.Validate(new[] {
                     " | w1 w2 |a x:1",
                     "0:-1:.3 | w2 w3"
                    },
                     "{\"adf\":[{\"someText\":\"w1 w2\", \"a\":{\"x\":1}, \"xxxxIgnoreMe\":2}, {\"someText\":\"w2 w3\"}], \"_labelindex\":1, \"_label_cost\":-1, \"_label_probability\":0.3}",
                     VowpalWabbitLabelComparator.ContextualBandit);

                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     " | w2 w3"
                    },
                     "{\"Age\":25,\"adf\":[{\"someText\":\"w1 w2\", \"a\":{\"x\":1}, \"xxxxIgnoreMe\":2}, {\"someText\":\"w2 w3\"}], \"_labelindex\":null}",
                     VowpalWabbitLabelComparator.ContextualBandit);
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonLabelExtraction()
        {
            using (var vw = new VowpalWabbit("--cb_adf --rank_all"))
            {
                using (var jsonSerializer = new VowpalWabbitJsonSerializer(vw))
                {
                    string eventId = null;
                    jsonSerializer.RegisterExtension((state, property) =>
                    {
                        Assert.AreEqual(property, "_eventid");
                        Assert.IsTrue(state.Reader.Read());

                        eventId = (string)state.Reader.Value;
                        return true;
                    });

                    jsonSerializer.Parse("{\"_eventid\":\"abc123\",\"a\":1,\"_label_cost\":-1,\"_label_probability\":0.3}");

                    Assert.AreEqual("abc123", eventId);

                    using (var examples = jsonSerializer.CreateExamples())
                    {
                        var single = examples as VowpalWabbitSingleLineExampleCollection;
                        Assert.IsNotNull(single);

                        var label = single.Example.Label as ContextualBanditLabel;
                        Assert.IsNotNull(label);

                        Assert.AreEqual(-1, label.Cost);
                        Assert.AreEqual(0.3, label.Probability, 0.0001);
                    }
                }

                using (var jsonSerializer = new VowpalWabbitJsonSerializer(vw))
                {
                    jsonSerializer.Parse("{\"_multi\":[{\"_text\":\"w1 w2\", \"a\":{\"x\":1}}, {\"_text\":\"w2 w3\"}], \"_labelindex\":1, \"_label_cost\":-1, \"_label_probability\":0.3}");

                    using (var examples = jsonSerializer.CreateExamples())
                    {
                        var multi = examples as VowpalWabbitMultiLineExampleCollection;
                        Assert.IsNotNull(multi);

                        Assert.AreEqual(2, multi.Examples.Length);
                        var label = multi.Examples[0].Label as ContextualBanditLabel;
                        Assert.AreEqual(0, label.Cost);
                        Assert.AreEqual(0, label.Probability);

                        label = multi.Examples[1].Label as ContextualBanditLabel;
                        Assert.IsNotNull(label);

                        Assert.AreEqual(-1, label.Cost);
                        Assert.AreEqual(0.3, label.Probability, 0.0001);
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("JSON")]
        public void TestJsonRedirection()
        {
            using (var validator = new VowpalWabbitExampleJsonValidator(new VowpalWabbitSettings("--cb_adf")))
            {
                validator.Validate(new[] {
                     "shared | Age:25",
                     " | w1 w2 |a x:1",
                     "0:-1:.3 | w2 w3"
                    },
                    "{\"_ignoreMe\":5,\"_sub\":{\"Age\":25,\"_multi\":[{\"_text\":\"w1 w2\", \"a\":{\"x\":1}}, {\"_text\":\"w2 w3\"}]}, \"_labelIndex\":1, \"_label_Cost\":-1, \"_label_Probability\":0.3}",
                    VowpalWabbitLabelComparator.ContextualBandit,
                    extension: (state, property) =>
                    {
                        if (!property.Equals("_sub"))
                            return false;

                        state.Parse();

                        return true;
                    });
            }
        }

        public class MyContext
        {
            [Feature]
            public int Feature { get; set; }

            [JsonProperty("_multi")]
            public IEnumerable<MyADF> Multi { get; set; }
        }

        public class MyADF
        {
            [Feature]
            public int Foo { get; set; }
        }

        [TestMethod]
        public void TestNumADFs()
        {
            var jsonDirectSerializer = VowpalWabbitSerializerFactory.CreateSerializer<MyContext>(new VowpalWabbitSettings { TypeInspector = JsonTypeInspector.Default })
                as IVowpalWabbitMultiExampleSerializerCompiler<MyContext>;

            Assert.IsNotNull(jsonDirectSerializer);
            Assert.AreEqual(3,
                jsonDirectSerializer.GetNumberOfActionDependentExamples(new MyContext { Multi = new MyADF[3] }));
        }
    }
}
