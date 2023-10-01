This is mostly just an exercise in operator overloading.

I wanted to take the following python/pandas code:

```python
df.loc[ df['SomeField] > 70, 'AnotherField'   ] = 'NewValue';
```

and make it work in C#. I can almost get there, except C# doesn't let you overload assignment operators.

So instead of assignment operators, I'm just providing a function.

```cs
df.loc( df["size"] < 50, "another" ).update("tiger");
```

Note: I just realized I'm doing parens for loc instead of brackets.

Next week!

Oh also: In order for "loc" to be followed by brackets, log needs to be a property that itself is an object that implements an index using an instance of DataList. We could have loc's get (assuming it's a property) return an instance of DataList, and then everything will be confined within the single class.
