unit Test;

type //начало описания класса

TParallel2  = class;

TParallel = class(BaseClass,IEnumerable)
  public 
    Fa: word; 
    Fb: word;
  private
    Fc: word;
  protected
    procedure Init(a, b, c : word);
    function Volume: word;
  private
    procedure Show;
end;
type

TParallel2 = class(BaseClass)
  public 
    Var1: TDateTime; 
    Var2: Integer;
  private
    Var3: word;
  protected
    function foo(g: Integer): word;
  private
    function Init(a, b, c : word): word;
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