unit Test;

type //начало описания класса
TParallel = class 
  public 
  Fa: word; 
  Fb: word;
  Fc: word;
  procedure Init(a, b, c : word);
  function Volume: word;
  procedure Show;
end;

var Form1: TForm1, Par1: TParallel;
implementation 
{$R *.dfm}

procedure TParallel.Init(a,b,c: word);
begin
  Fa:=a;
  Fb:=b;
  Fc:=c;
end;

procedure TParallel.Show;
begin
  ShowMessage('Объем равен' + IntToStr(Volumde) + #13 +
  + 'Ширина - Поле Fa' + IntToStr(Fa) + #13 +
  + 'Длина - Поле Fb' + IntToStr(Fb) + #13 +
  + 'Высота-Поле Fс' + IntToStr(Fс));
end;

procedure TParallel.Volume: word;
begin
  result:=Fa*Fb*Fc;
end;