using System;
using System.Collections.Generic;
using static System.Console;

namespace OpenClosedPrinciple
{
    // the open/closed principle states "software entities (classes, modules, functions, etc.)
    // should be open for extension, but closed for modification"
    // that is, such an entity can allow its behaviour to be extended without modifying its source code.

    class Program
    {
        public enum Color
        {
            Red, Green, Blue
        }
        public enum Size
        {
            Small, Medium, Large, Yuge
        }
        public class Product
        {
            public string Name;
            public Color Color;
            public Size Size;

            public Product(string name, Color color, Size size)
            {
                Name = name ?? throw new ArgumentNullException(paramName: nameof(name));
                Color = color;
                Size = size;
            }
        }
        public class ProductFilter
        {
            // let's suppose we don't want ad-hoc queries on products
            public IEnumerable<Product> FilterByColor(IEnumerable<Product> products, Color color)
            {
                foreach (var p in products)
                    if (p.Color == color)
                        yield return p;
            }

            public static IEnumerable<Product> FilterBySize(IEnumerable<Product> products, Size size)
            {
                foreach (var p in products)
                    if (p.Size == size)
                        yield return p;
            }

            public static IEnumerable<Product> FilterBySizeAndColor(IEnumerable<Product> products, Size size, Color color)
            {
                foreach (var p in products)
                    if (p.Size == size && p.Color == color)
                        yield return p;
            } // state space explosion
              // 3 criteria = 7 methods

            // OCP = open for extension but closed for modification
        }


        public interface ISpecification<T>
        {
            bool IsSatisfied(T t);
        }
        public interface IFilter<T>
        {
            IEnumerable<T> Filter(IEnumerable<T> items, ISpecification<T> spec);
        }
        public class ColorSpecification : ISpecification<Product>
        {
            Color _color;
            public ColorSpecification(Color color)
            {
                this._color = color;
            }
            public bool IsSatisfied(Product t)
            {
                return t.Color == _color;
            }
        }
        public class SizeSpecification : ISpecification<Product>
        {
            Size _size;
            public SizeSpecification(Size size)
            {
                this._size = size;
            }
            public bool IsSatisfied(Product t)
            {
                return t.Size == _size;
            }
        }
        public class AndSpecification<T> : ISpecification<T>
        {
            ISpecification<T> _first, _second;

            public AndSpecification(ISpecification<T> first, ISpecification<T> second)
            {
                this._first = first;
                this._second = second;
            }

            public bool IsSatisfied(T t)
            {
                return _first.IsSatisfied(t) && _second.IsSatisfied(t);
            }
        }
        public class BetterFilter : IFilter<Product>
        {
            public IEnumerable<Product> Filter(IEnumerable<Product> items, ISpecification<Product> spec)
            {
                foreach (var item in items)
                {
                    if (spec.IsSatisfied(item))
                        yield return item;
                }
            }
        }



        static void Main(string[] args)
        {
            var apple = new Product("Apple", Color.Green, Size.Small);
            var tree = new Product("Tree", Color.Green, Size.Large);
            var house = new Product("House", Color.Blue, Size.Large);

            Product[] products = { apple, tree, house };

            var pf = new ProductFilter();
            WriteLine("Green products (old):");
            foreach (var p in pf.FilterByColor(products, Color.Green))
                WriteLine($" - {p.Name} is green");

            // ^^ BEFORE

            // vv AFTER
            WriteLine("Green products (new):");
            var bf = new BetterFilter();
            foreach (var p in bf.Filter(products, new ColorSpecification(Color.Green)))
                WriteLine($" - {p.Name} is green");

            WriteLine("Large Blue items (new):");
            foreach (var p in bf.Filter(products, new AndSpecification<Product>(new ColorSpecification(Color.Blue), new SizeSpecification(Size.Large))))
            {
                WriteLine($" - {p.Name} is big and blue");
            }
        }

    }
}
