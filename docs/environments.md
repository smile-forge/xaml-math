Environments (`\begin` and `\end`)
==================================
XAML-Math supports _environments_: ability to introduce certain context for the nested markup.

`\begin{environment-name}` starts an environment named `environment-name`, `\end{environment-name}` ends it. Every `\begin` should be followed by a corresponding `\end`. Environments could be nested.

List of currently supported environment names:

- `pmatrix`, `bmatrix`, `Bmatrix`, `vmatrix`, `Vmatrix`: each works the same as the [corresponding matrix command][docs.matrices], differing only in the delimiters that surround the matrix.

  Examples:
  ```tex
  \begin{pmatrix} a & b & c \\ d & e & f \end{pmatrix}
  ```
  This works the same as:
  ```tex
  \pmatrix{a & b & c \\ d & e & f}
  ```

- `matrix`, `smallmatrix`: a matrix without delimiters. `smallmatrix` is the same layout set in script size, so it fits inside a line of text:
  ```tex
  \left( \begin{smallmatrix} a & b \\ c & d \end{smallmatrix} \right)
  ```

- `cases`: a piecewise definition, the environment form of the [`\cases` command][docs.matrices]:
  ```tex
  f(x) = \begin{cases} 1 & x > 0 \\ 0 & x \leq 0 \end{cases}
  ```

- `align`, `align*`, `aligned`, `split`: used for equation alignment.

  For example, this will make the left and right parts of equations aligned:
  ```tex
  \begin{align} x+1 &= y + 1 \\ x &= y-1 \end{align}
  ```

  And this will allow to put the equations into several columns:
  ```tex
  \begin{align} x+1 &= y + 1 & a &= b + 1 \\ x &= y-1 & b + a &= c \end{align}
  ```

- `gather`, `gather*`, `gathered`: like `align`, but each row is centred rather than aligned on `&`:
  ```tex
  \begin{gather} a^2 + b^2 = c^2 \\ e^{i\pi} + 1 = 0 \end{gather}
  ```

- `array`: the only environment that takes an argument - a column preamble, giving each column its
  own alignment with `l`, `c` or `r`, and a rule at any boundary marked `|`. `\hline` draws a rule
  between rows. A preamble asking for anything else (`p{...}`, `@{...}`) is an error rather than
  something silently dropped.
  ```tex
  \left[\begin{array}{cc|c} 1 & 0 & 3 \\ 0 & 1 & 4 \end{array}\right]
  ```

[docs.matrices]: matrices.md
