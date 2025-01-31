#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests
{
   public class GridifyExtentionsShould
   {

      private List<TestClass> _fakeRepository;
      public GridifyExtentionsShould() => _fakeRepository = new List<TestClass>(GetSampleData());

#region "ApplyFiltering"
      [Fact]
      public void ApplyFiltering_SingleField()
      {
         var gq = new GridifyQuery() { Filter = "name==John" };
         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();
         var expected = _fakeRepository.Where(q => q.Name == "John").ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_SingleGuidField()
      {
         var guidString = "e2cec5dd-208d-4bb5-a852-50008f8ba366";
         var guid = Guid.Parse(guidString);
         var gq = new GridifyQuery() { Filter = "guid==" + guidString };
         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();
         var expected = _fakeRepository.Where(q => q.guid == guid).ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_SingleBrokenGuidField()
      {
         var brokenGuidString = "e2cec5dd-208d-4bb5-a852-";
         var gq = new GridifyQuery() { Filter = "guid==" + brokenGuidString };

         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();

         Assert.False(actual.Any());
      }

      
      [Fact]
      public void ApplyFiltering_SingleBrokenGuidField_NotEqual()
      {
         var brokenGuidString = "e2cec5dd-208d-4bb5-a852-";
         var gq = new GridifyQuery() { Filter = "guid!=" + brokenGuidString };

         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();

         Assert.True(actual.Any());
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      public void ApplyFiltering_NullOrEmptyFilter_ShouldSkip(string? filter)
      {
         var gq = new GridifyQuery();
         if (filter == null)
            gq = null;
         else
            gq.Filter = filter;

         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();

         var expected = _fakeRepository.ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_MultipleCondition()
      {
         var gq = new GridifyQuery() { Filter = "name==Jack|name==Rose|id>>7" };
         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();
         var expected = _fakeRepository.Where(q => q.Name == "Jack" || q.Name == "Rose" || q.Id > 7).ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_ComplexWithParenthesis()
      {
         var gq = new GridifyQuery() { Filter = "(name=*J|name=*S),(Id<<5)" };
         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();
         var expected = _fakeRepository.Where(q => (q.Name.Contains("J") || q.Name.Contains("S")) && q.Id < 5).ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_UsingChildClassProperty()
      {
         var gq = new GridifyQuery() { Filter = "Child-Name==Bob" };
         var gm = new GridifyMapper<TestClass>()
            .GenerateMappings()
            .AddMap("Child-name", q => q.ChildClass.Name);

         var actual = _fakeRepository.AsQueryable()
            .Where(q => q.ChildClass != null)
            .ApplyFiltering(gq, gm)
            .ToList();

         var expected = _fakeRepository.Where(q => q.ChildClass != null && q.ChildClass.Name == "Bob").ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Fact]
      public void ApplyFiltering_CustomConvertor()
      {
         var gq = new GridifyQuery() { Filter = "name==liam" };
         var gm = new GridifyMapper<TestClass>()
            .GenerateMappings()
            .AddMap("name", q => q.Name, q => q.ToUpper()); // useing client side Custom convertor

         var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq, gm)
            .ToList();

         var expected = _fakeRepository.Where(q => q.Name == "LIAM").ToList();
         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

#endregion

#region "ApplyOrdering"
      [Fact]
      public void ApplyOrdering_SortBy_Ascending()
      {
         var gq = new GridifyQuery() { SortBy = "name", IsSortAsc = true };
         var actual = _fakeRepository.AsQueryable()
            .ApplyOrdering(gq)
            .ToList();
         var expected = _fakeRepository.OrderBy(q => q.Name).ToList();
         Assert.Equal(expected, actual);

      }

      [Fact]
      public void ApplyOrdering_SortBy_Descending()
      {
         var gq = new GridifyQuery() { SortBy = "Name", IsSortAsc = false };
         var actual = _fakeRepository.AsQueryable()
            .ApplyOrdering(gq)
            .ToList();
         var expected = _fakeRepository.OrderByDescending(q => q.Name).ToList();
         Assert.Equal(expected, actual);
      }

      [Fact]
      public void ApplyOrdering_SortUsingChildClassProperty()
      {
         var gq = new GridifyQuery() { SortBy = "Child-Name", IsSortAsc = false };
         var gm = new GridifyMapper<TestClass>()
            .GenerateMappings()
            .AddMap("Child-Name", q => q.ChildClass.Name);

         var actual = _fakeRepository.AsQueryable().Where(q => q.ChildClass != null)
            .ApplyOrdering(gq, gm)
            .ToList();

         var expected = _fakeRepository.Where(q => q.ChildClass != null)
            .OrderByDescending(q => q.ChildClass.Name).ToList();

         Assert.Equal(expected, actual);
      }

      [Fact]
      public void ApplyOrdering_EmptySortBy_ShouldSkip()
      {
         var gq = new GridifyQuery() { };
         var actual = _fakeRepository.AsQueryable()
            .ApplyOrdering(gq)
            .ToList();
         var expected = _fakeRepository.ToList();
         Assert.Equal(expected, actual);
      }

      [Fact]
      public void ApplyOrdering_NullGridifyQuery_ShouldSkip()
      {
         var actual = _fakeRepository.AsQueryable()
            .ApplyOrdering(null)
            .ToList();
         var expected = _fakeRepository.ToList();
         Assert.Equal(expected, actual);
      }
#endregion

#region "ApplyPaging"
      [Fact]
      public void ApplyPaging_UsingDefaultValues()
      {
         var gq = new GridifyQuery();
         var actual = _fakeRepository.AsQueryable()
            .ApplyPaging(gq)
            .ToList();

         // just returning first page with default size 
         var expected = _fakeRepository.Take(GridifyExtensions.DefaultPageSize).ToList();

         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }

      [Theory]
      [InlineData(1, 5)]
      [InlineData(2, 5)]
      [InlineData(1, 10)]
      [InlineData(4, 3)]
      [InlineData(5, 3)]
      [InlineData(1, 15)]
      [InlineData(20, 10)]
      public void ApplyPaging_UsingCustomValues(short page, int pageSize)
      {
         var gq = new GridifyQuery() { Page = page, PageSize = pageSize };
         var actual = _fakeRepository.AsQueryable()
            .ApplyPaging(gq)
            .ToList();

         var skip = (page - 1) * pageSize;
         var expected = _fakeRepository.Skip(skip)
            .Take(pageSize).ToList();

         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
      }
#endregion

#region "Other"
      [Theory]
      [InlineData(1, 5, true)]
      [InlineData(2, 5, false)]
      [InlineData(1, 10, true)]
      [InlineData(4, 3, false)]
      [InlineData(5, 3, true)]
      [InlineData(1, 15, false)]
      [InlineData(20, 10, true)]
      public void ApplyOrderingAndPaging_UsingCustomValues(short page, int pageSize, bool isSortAsc)
      {
         var gq = new GridifyQuery() { Page = page, PageSize = pageSize, SortBy = "Name", IsSortAsc = isSortAsc };
         // actual
         var actual = _fakeRepository.AsQueryable()
            .ApplyOrderingAndPaging(gq)
            .ToList();

         // expected
         var skip = (page - 1) * pageSize;
         var expectedQuery = _fakeRepository.AsQueryable();
         if (isSortAsc)
            expectedQuery = expectedQuery.OrderBy(q => q.Name);
         else
            expectedQuery = expectedQuery.OrderByDescending(q => q.Name);
         var expected = expectedQuery.Skip(skip).Take(pageSize).ToList();

         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
      }
#endregion

#region "Data"
      private List<TestClass> GetSampleData()
      {
         var lst = new List<TestClass>();
         lst.Add(new TestClass(1, "John", null, Guid.NewGuid()));
         lst.Add(new TestClass(2, "Bob", null, Guid.NewGuid()));
         lst.Add(new TestClass(3, "Jack", (TestClass) lst[0].Clone()));
         lst.Add(new TestClass(4, "Rose", null, Guid.Parse("e2cec5dd-208d-4bb5-a852-50008f8ba366")));
         lst.Add(new TestClass(5, "Ali", null));
         lst.Add(new TestClass(6, "Hamid", (TestClass) lst[0].Clone(), Guid.Parse("de12bae1-93fa-40e4-92d1-2e60f95b468c")));
         lst.Add(new TestClass(7, "Hasan", (TestClass) lst[1].Clone()));
         lst.Add(new TestClass(8, "Farhad", (TestClass) lst[2].Clone()));
         lst.Add(new TestClass(9, "Sara", null));
         lst.Add(new TestClass(10, "Jorge", null));
         lst.Add(new TestClass(11, "joe", null));
         lst.Add(new TestClass(12, "jimmy", (TestClass) lst[0].Clone()));
         lst.Add(new TestClass(13, "Nazanin", null));
         lst.Add(new TestClass(14, "Reza", null));
         lst.Add(new TestClass(15, "Korosh", (TestClass) lst[0].Clone()));
         lst.Add(new TestClass(16, "Kamran", (TestClass) lst[1].Clone()));
         lst.Add(new TestClass(17, "Saeid", (TestClass) lst[2].Clone()));
         lst.Add(new TestClass(18, "jessica", null));
         lst.Add(new TestClass(19, "Pedram", null));
         lst.Add(new TestClass(20, "Peyman", null));
         lst.Add(new TestClass(21, "Fereshte", null));
         lst.Add(new TestClass(22, "LIAM", null));
         return lst;
      }
#endregion
   }
}