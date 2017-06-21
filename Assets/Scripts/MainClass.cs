using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;


[System.Serializable]
public class Hero{
	public int id;
	public string name;
	public float hp;
	public float atk;
	public float def;
	public float spd;
	public int all;
	public int win;
	public float per;
	public Hero()
	{
	}	
	public Hero(int _id)
	{
		id=_id;
	}
	public Hero(Hero h)
	{
		id = h.id;
		name = h.name;
		hp = h.hp;
		atk = h.atk;
		def = h.def;
		spd = h.spd;
		all = h.all;
		win = h.win;
		per = h.per;
	}

	public Hero GetHero(int lv)
	{
		Hero h = new Hero();	
		h.hp=hp*lv/100;
		h.atk=atk*lv/100;
		h.def=def*lv/100;
		return h;
	}
}

[System.Serializable]
public class HeroStatus
{
	public bool isDead;
	public bool wasAtt;
	public TeamEnum team;
	public float hp;
	public int pow;
	public override string ToString ()
	{
		return string.Format ("HeroStatus:Dead:{0}  Team:{1}  HP:{2}",isDead,team,hp);
	}
}

[System.Serializable]
public class HeroInBattle
{
	public Hero hero;
	public HeroStatus status;

	public HeroInBattle(Hero _hero)
	{
		hero = new Hero(_hero);
		ResetStatus();
	}

	public HeroInBattle(HeroInBattle _hero)
	{
		hero = new Hero(_hero.hero);
		ResetStatus();
		status.team = _hero.status.team;
	}

	public void ResetStatus()
	{
		if(status==null)
			status = new HeroStatus();
		status.hp = hero.hp;
		status.isDead = false;
		status.pow = 50;
	}
}


[System.Serializable]
public class Team{

	public List<HeroInBattle> heros;
	public TeamEnum team;
	public Team ()
	{
		heros = new List<HeroInBattle>();
	}

	public Team (Team a)
	{
		team  = a.team;
		heros = new List<HeroInBattle>();
	//	heros = a.heros;
		foreach(HeroInBattle h in a.heros)
			heros.Add(new HeroInBattle(h));
		SetTeam(team);
	}

	public void SetTeam(Team teamB)
	{
		team = teamB.team;
		heros = new List<HeroInBattle>();
		foreach(HeroInBattle hero in teamB.heros)
		{
			heros.Add(new HeroInBattle(hero));
		}
	}

	public void SetTeam(TeamEnum _team)
	{
		team = _team;
		foreach(HeroInBattle hero in heros)
			hero.status.team = team;
	}

	public void ResetStatus()
	{
		foreach(HeroInBattle h in heros)
			h.ResetStatus();
	}

	public void DefaultTeam(Hero hero)
	{
		heros.Clear();
		heros.Add(new HeroInBattle(hero));
		heros.Add(new HeroInBattle(hero));
		heros.Add(new HeroInBattle(hero));
		heros.Add(new HeroInBattle(hero));
		heros.Add(new HeroInBattle(hero));
		heros.Add(new HeroInBattle(hero));
	}

	public HeroInBattle GetFirstHeroIsLive()
	{
		for(int i=0;i<6;i++)
			if(heros[i].status.isDead == false)
				return heros[i];
		return null;
	}

	public bool IsDeadAll()
	{
		for(int i=0;i<6;i++)
			if(heros[i].status.isDead == false)
				return false;
		return true;
	}

	public void Print()
	{
		foreach(HeroInBattle h in heros)
		{
			Debug.Log(h.status.ToString() + team);	
		}
	}
}

[System.Serializable]
public enum Result
{
	WIN,
	LOSE
}

[System.Serializable]
public enum TeamEnum
{
	teamA,
	teamB
}

public class MainClass : MonoBehaviour {
	const string fileName = "Assets\\Resources\\heros.xlsx";
	int numberHero=60;
	int staRow;
	int idCol;
	int nameCol;
	int hpCol;
	int atkCol;
	int defCol;
	int spdCol;
	int claCol;
	int proCol;
	int allCol;
	int winCol;


	public int loglv=5;
	public List<Hero> heros;
	public Team teamA;
	public Team teamB;

	// Use this for initialization
	void Start () {
		FileInfo newFile = new FileInfo(fileName);
		if (newFile.Exists) 
		{
			ExcelPackage excel = new ExcelPackage(newFile);
			ExcelWorksheets sheets = excel.Workbook.Worksheets;
			LoadDefineValue(sheets["define"]);
			InitAllHeroes(sheets["heros"]);
			Print(System.DateTime.Now.ToString(),1);
			for(int i=0;i<200;i++)
			{
				//Print("Battle "+i,2);
				ResetBattle();
				InitTeam();
				teamA=TrainningTeam2(teamA);
				//PrintName(teamA,99);
				teamB=TrainningTeam2(teamB);
				//PrintName(teamB,99);


//				teamA = TrainningTeam(teamA);
//				teamB = TrainningTeam(teamB);
				teamB.SetTeam(TeamEnum.teamB);
				TeamEnum teamwin = Battle(teamA,teamB);
				AddData(teamwin);
			}
			Print(System.DateTime.Now.ToString(),1);
			SaveData(sheets["heros"]);
			excel.Save();

		}
		else 
		{
			Print("File does not exists",1);
		}
	}

	#region initdata
	void LoadDefineValue(ExcelWorksheet sheet)
	{
		//Debug.Log("File exists" + sheet.GetValue(1,2).ToString());
		numberHero =GetInt(sheet.GetValue(1,2));
		staRow =	GetInt(sheet.GetValue(2,2));
		idCol  =	GetInt(sheet.GetValue(3,2));
		nameCol=	GetInt(sheet.GetValue(4,2));
		hpCol  =	GetInt(sheet.GetValue(5,2));
		atkCol =	GetInt(sheet.GetValue(6,2));
		defCol =	GetInt(sheet.GetValue(7,2));
		spdCol =	GetInt(sheet.GetValue(8,2));
		claCol =	GetInt(sheet.GetValue(9,2));
		proCol =	GetInt(sheet.GetValue(10,2));
		allCol =	GetInt(sheet.GetValue(11,2));
		winCol =	GetInt(sheet.GetValue(12,2));

	}

	public void InitAllHeroes(ExcelWorksheet sheet)
	{
		heros = new List<Hero>();
		for(int i=0;i<numberHero;i++)
		{
			heros.Add(LoadHeroByRow(sheet,i+staRow));
		}
	}

	public Hero LoadHeroByRow(ExcelWorksheet sheet,int row)
	{
		Hero hero = new Hero();
		hero.id = GetInt(sheet.GetValue(row,idCol));
		hero.name = sheet.GetValue(row,nameCol).ToString();
		hero.hp = GetFloat(sheet.GetValue(row,hpCol));
		hero.atk = GetFloat(sheet.GetValue(row,atkCol));
		hero.def = GetFloat(sheet.GetValue(row,defCol));
		hero.spd = GetFloat(sheet.GetValue(row,spdCol));
		return hero;
	}

	int GetInt(object o)
	{
		return int.Parse(o.ToString());
	}
	float GetFloat(object o)
	{
		return float.Parse(o.ToString());
	}
	#endregion

	#region battle
	public void ResetBattle()
	{
		teamA = new Team();
		teamB = new Team();
	}
	public void InitTeam()
	{
		for(int i=0;i<6;i++)
			teamA.heros.Add(new HeroInBattle(heros[Random.Range(0,numberHero)]));
		teamA.SetTeam(TeamEnum.teamA);

		for(int i=0;i<6;i++)
			teamB.heros.Add(new HeroInBattle(heros[Random.Range(0,numberHero)]));
		teamB.SetTeam(TeamEnum.teamB);
//		for(int i=0;i<6;i++)
//			teamA.heros[i] = new HeroInBattle(heros[Random.Range(0,numberHero)]);
//		teamA.team = TeamEnum.teamA;
//
//		for(int i=0;i<6;i++)
//			teamB.heros[i] = new HeroInBattle(heros[Random.Range(0,numberHero)]);
//		teamB.team = TeamEnum.teamB;
	}

	public Team TrainningTeam2(Team teamA)
	{
		//teamA -> temp
		//temp move a hero to teamA
		//if(
//		for(int i =0;i< teamA.heros.Count ; i++)
//		{
//			HeroInBattle h = teamA.heros[i];
//			teamA.heros.RemoveAt(i);
//
//		}

		Team bestTeam = new Team();
		bestTeam = new Team(teamA);

		Team teamTemp = new Team();
		teamTemp = new Team(teamA);
		teamA = new Team();
		teamA.SetTeam(teamTemp.team);
		int lv = 70;
		//PrintCheckTime("TrainningTeam3  :  ");
		TrainningTeam3(teamA,teamTemp,ref bestTeam,ref lv);
	//	PrintCheckTime("else thing  :  ");
		teamA = bestTeam;
		//Debug.Log("best team is:");
		//PrintName(teamA,lv);
		//PrintName(bestTeam,lv);
		return bestTeam;
	}
	Team wtfteam;
	public void PrintName(Team a,int lv)
	{
		Debug.Log("team lv"+lv);
		foreach(HeroInBattle h in a.heros)
			Debug.LogFormat("name:{0}  hp:{1}  atk:{2}", h.hero.name,h.hero.hp,h.hero.atk);
	}

	public void TrainningTeam3(Team teamA,Team teamTemp,ref Team bestTeam,ref int lv)
	{
		if(teamA.heros.Count >=6)
		{
			//start battle
			TeamEnum enemyTeam = TeamEnum.teamB;
			if(teamA.team == TeamEnum.teamB)
				enemyTeam = TeamEnum.teamA;
			Hero heroDefaul=heros[0].GetHero(lv);
			Team teamDefault =new Team();
			teamDefault.DefaultTeam(heroDefaul);
			teamDefault.SetTeam(enemyTeam);

			while( Battle(teamA,teamDefault) == teamA.team) 
			{
				//Debug.Log("-----------------------------------------------");
				//PrintName(teamA,lv);



				teamA.ResetStatus();
				bestTeam  = new Team(teamA);
				//PrintName(bestTeam,lv);
				lv++;
				heroDefaul=heros[0].GetHero(lv);
				teamDefault.DefaultTeam(heroDefaul);
				teamDefault.SetTeam(enemyTeam);
				teamDefault.ResetStatus();
//
//				if(lv>1000)
//				{
//					Debug.Log("<color=#ff0000>wrong something</color>");
//					break;
//				}

			}
		}
		else 
		{
			for(int i = 0 ; i < teamTemp.heros.Count; i++)
			{
				teamA.heros.Add(teamTemp.heros[i]);
				List<HeroInBattle> tempList = new List<HeroInBattle>();
				foreach(HeroInBattle h in teamTemp.heros)
					tempList.Add(h);
				teamTemp.heros.RemoveAt(i);
				TrainningTeam3(teamA,teamTemp,ref bestTeam,ref lv);
				teamTemp.heros = tempList;
				teamA.heros.RemoveAt(teamA.heros.Count-1);
			}
		}

	}


	public Team TrainningTeam(Team teamA)
	{
		TeamEnum enemyTeam = TeamEnum.teamB;
		if(teamA.team == TeamEnum.teamB)
			enemyTeam = TeamEnum.teamA;

		//fight with default team lv...
		//lv 92
		//lv 100
		//lv 108
		int lv=70;
		Hero heroDefaul=heros[0].GetHero(lv);
		Team teamDefault =new Team();
		teamDefault.DefaultTeam(heroDefaul);
		teamDefault.SetTeam(enemyTeam);
		Team teamTrain = new Team(teamA);
		Team bestTeam = new Team(teamA);
		for(int i=0;i<Mathf.Pow(6,6);i++)
		{
			int[] a = new int[6];
			int k=i;
			for(int j =0;j<6;j++)
			{
				a[j]=k%6;
				k/=6;
			}
			if(a[0]!=a[1] && a[0]!=a[2] && a[0]!=a[3] && a[0]!=a[4] &&
				a[0]!=a[5] && a[1]!=a[2] && a[1]!=a[3] && a[1]!=a[4] &&
				a[1]!=a[5] && a[2]!=a[3] && a[2]!=a[4] && a[2]!=a[5] && 
				a[3]!=a[4] && a[3]!=a[5] && a[4]!=a[5])
			{
				teamTrain.heros[0] = new HeroInBattle(teamA.heros[a[0]]);
				teamTrain.heros[1] = new HeroInBattle(teamA.heros[a[1]]);
				teamTrain.heros[2] = new HeroInBattle(teamA.heros[a[2]]);
				teamTrain.heros[3] = new HeroInBattle(teamA.heros[a[3]]);
				teamTrain.heros[4] = new HeroInBattle(teamA.heros[a[4]]);
				teamTrain.heros[5] = new HeroInBattle(teamA.heros[a[5]]);
				while( Battle(teamTrain,teamDefault) == teamA.team) 
				{
					if(lv>=1000)
					{
						//Debug.Log("BEFOR COMBAT");
						//teamDefault.Print();
						//teamTrain.Print();
					}
					teamTrain.ResetStatus();
					bestTeam  = new Team(teamTrain);
					lv+=1;
					heroDefaul=heros[0].GetHero(lv);
					teamDefault.DefaultTeam(heroDefaul);
					teamDefault.SetTeam(enemyTeam);
					teamDefault.ResetStatus();
					if(lv>1000)
					{
						//Debug.Log("AFTER COMBAT");
						//teamDefault.Print();
						//teamTrain.Print();
						break;
					}
				//	teamDefault.Print();
				//	teamTrain.Print();

				}
				{
					teamTrain.ResetStatus();
					//teamDefault.SetTeam(TeamEnum.teamB);
					teamDefault.ResetStatus();
				//	teamDefault.Print();
				//	teamTrain.Print();
				}
			
			}
		}
//		Debug.Log("lv"+lv);
//		Debug.Log(bestTeam.heros[0].hero.name);
//		Debug.Log(bestTeam.heros[1].hero.name);
//		Debug.Log(bestTeam.heros[2].hero.name);
//		Debug.Log(bestTeam.heros[3].hero.name);
//		Debug.Log(bestTeam.heros[4].hero.name);
//		Debug.Log(bestTeam.heros[5].hero.name);
		return bestTeam;
	}

	public TeamEnum Battle(Team teamA,Team teamB)
	{
		//PrintCheckTime("start");
		teamA.ResetStatus();
		teamB.ResetStatus();
		List<HeroInBattle> sortSpd = new List<HeroInBattle>();
		for(int i=0;i<6;i++)
		{
			sortSpd.Add(teamA.heros[i]);
		}

		for(int i=0;i<6;i++)
		{
			sortSpd.Add(teamB.heros[i]);
		}

		for(int i=0;i<11;i++)
		{
//			if(sortSpd[i].hero.id==3)
//			{
//				Print("tracking");
//			}
			for(int j=i+1;j<12;j++)
			{
				if(sortSpd[i].hero.spd<sortSpd[j].hero.spd)
				{
					HeroInBattle hero = sortSpd[i];
					sortSpd[i] = sortSpd[j];
					sortSpd[j] = hero;
				}
			}
		}


		for(int i=0;i<15;i++)
		{
			for(int j=0;j<12;j++)
			{
				HeroInBattle heroAtt = sortSpd[j];
				if(heroAtt.status.isDead == false)
				{
					HeroInBattle heroDef = null;
					Team teamDef = null;
					if(heroAtt.status.team == teamA.team)
						teamDef = teamB;
					else 
						teamDef = teamA;
					heroDef = teamDef.GetFirstHeroIsLive();
//					if(heroAtt==null || heroDef==null)
//					{
//						//Print("turn "+i.ToString() +"   "+j.ToString()+"   "+heroAtt.status.team,1);
//						teamA.Print();
//						teamB.Print();
//					}
					if(heroAtt.status.pow>=100)
					{
						heroDef.status.hp -= Mathf.Max((heroAtt.hero.atk*2-heroDef.hero.def),0);
						heroDef.status.pow+=10;
						heroAtt.status.pow=0;
						if(heroDef.status.hp <=0)
						{
							heroDef.status.isDead = true;
							if(teamDef.IsDeadAll())
							{
								Print (heroAtt.status.team.ToString() + "is win in turn "+i.ToString(),3);
								return heroAtt.status.team;
							}
						}
					}
					else
					{
						heroDef.status.hp -= Mathf.Max((heroAtt.hero.atk-heroDef.hero.def),0);
						heroDef.status.pow+=10;
						heroAtt.status.pow+=50;
						if(heroDef.status.hp <=0)
						{
							heroDef.status.isDead = true;
							if(teamDef.IsDeadAll())
							{
							//	Print (heroAtt.status.team.ToString() + "is win in turn "+i.ToString(),3);

								return heroAtt.status.team;
							}
						}
					}


				}
			}
			//gethero
			//att
			//get
			//
			//..
			//..
			//..
			//reset
		}
	//	Print("Draw battle, team B win",3);

		return teamB.team;
	}


	public void AddData(TeamEnum team)
	{
		for(int i=0;i<6;i++)
		{
			heros[teamA.heros[i].hero.id].all++;
		}
		for(int i=0;i<6;i++)
		{
			heros[teamB.heros[i].hero.id].all++;
		}
		Team teamWin =null;
		if(team == TeamEnum.teamA)
			teamWin = teamA;
		else
			teamWin = teamB;
		for(int i=0;i<6;i++)
		{
			heros[teamWin.heros[i].hero.id].win++;
		}

	}
	#endregion

	#region savedata
	public void SaveData(ExcelWorksheet sheet)
	{
		for(int i=0;i<numberHero;i++)
		{
			sheet.Cells[staRow+i,winCol+3].Value=heros[i].name;
			sheet.Cells[staRow+i,allCol].Value=heros[i].all;
			sheet.Cells[staRow+i,winCol].Value=heros[i].win;
			heros[i].per = (float)(heros[i].win)/heros[i].all;
		}
	}
	#endregion

	void Print(string s,int _loglv=3)
	{
		if(loglv>=_loglv)
			Debug.Log(s);
	}

	//string defaultMess = "Start Time: ";
	void PrintCheckTime(string s = "TIME: ")
	{
		Debug.Log("<b>"+s + System.DateTime.Now.Millisecond+"</b>");
	//	defaultMess = s;
	}
}


/// update all skill
/// update all buff
/// update all item
/// update all pet
/// update all khac che
/// crit dame pow+=20
/// hit
/// doge
/// def