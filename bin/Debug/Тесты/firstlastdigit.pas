program firstlastdigit;
var n, n1, p, a, i : integer;
begin
  n := 25615;
  a := n;
  i := 1;
  p := n mod 10; { последняя цифра введенного числа }
  while n >= 10 do
  begin
    i := i * 10;
    n := n div 10;
  end;
  n1 := a - n*i - p + n + p * i;
  write ('Число после перестановки первой и последней цифр числа = ');
  writeln(n1);
end.