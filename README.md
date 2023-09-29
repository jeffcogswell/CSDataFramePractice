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

