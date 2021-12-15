program operations_str;
var s1, s2: string;
begin
    s1 := 'Кот';
    s2 := 'Пёс';
    if s1 < s2 then
      s1 := s1 + 'o' + s2
    else
      s2 := s2 + 'o' + s1;
    writeln (s1);
    writeln (s2);
end.