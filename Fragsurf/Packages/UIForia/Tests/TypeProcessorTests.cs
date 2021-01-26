using System;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UnityEngine;

[TestFixture]
public class TypeProcessorTests {

    private class Thing {

        public class SubThing { }

        public enum NestedEnum { }

    }
    
    
    [Test]
    public void ResolveArrayType() {
        TypeLookup lookup = new TypeLookup();
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.isArray = true;
        Type t = TypeProcessor.ResolveType(lookup);
        Assert.AreEqual(typeof(string[]), t);
    }

    [Test]
    public void ResolveWithNamespaceNoOrigin() {
        Type t = TypeProcessor.ResolveType("Color", new List<string>() {"UnityEngine"});
        Assert.AreEqual(typeof(Color), t);

        Type t1 = TypeProcessor.ResolveType("SortedList", new List<string>() {"System.Collections"});
        Assert.AreEqual(typeof(System.Collections.SortedList), t1);
        
    }

    [Test]
    public void ResolveTypeFromTypeLookup() {
        TypeLookup lookup = new TypeLookup();
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.generics = null;
        Type t0 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(string), t0);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "string"
            }
        };
        Type t3 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t3);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "String",
                namespaceName = "System"
            }
        };
        Type t4 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t4);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                namespaceName = "System.Collections.Generic",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                        namespaceName = "UnityEngine"
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        namespaceName = "System.Collections.Generic",
                        generics = new[] {
                            new TypeLookup() {
                                typeName = "float"
                            },
                            new TypeLookup() {
                                typeName = "int"
                            }
                        }
                    }
                }
            }
        };
        Type t5 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t5);

        lookup.typeName = "List";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        generics = new[] {
                            new TypeLookup() {
                                typeName = "float"
                            },
                            new TypeLookup() {
                                typeName = "int"
                            }
                        }
                    }
                }
            }
        };
        
        Type t6 = TypeProcessor.ResolveType(lookup, new List<string>() {"System.Collections.Generic", "UnityEngine"});
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t6);
    }

   

}