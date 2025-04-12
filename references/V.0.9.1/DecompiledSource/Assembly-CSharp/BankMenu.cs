using TMPro;
using UnityEngine;

public class BankMenu : MonoBehaviour
{
	public static BankMenu menu;

	public GameObject window;

	public GameObject amountButtons;

	public GameObject confirmConversionWindow;

	public TextMeshProUGUI AccountTypeTitle;

	public TextMeshProUGUI amountInAccount;

	public TextMeshProUGUI amountChanging;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI dinkConversionTotal;

	public TextMeshProUGUI permitPointConversionTotal;

	public bool bankOpen;

	public int accountBalance;

	public int difference;

	public ulong accountOverflow;

	private bool depositing = true;

	private bool donating;

	public bool converting;

	private string amount = "";

	public InvButton closeWindowButton;

	public bool isAtm;

	public static int billion = 1000000000;

	private void Awake()
	{
		menu = this;
	}

	public void open()
	{
		AccountTypeTitle.text = ConversationGenerator.generate.GetJournalNameByTag("Account Balance");
		window.gameObject.SetActive(value: true);
		bankOpen = true;
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
		updateAccountAmounts();
		converting = false;
		isAtm = false;
	}

	public void OpenAsATM()
	{
		open();
		withdrawButton();
		amountButtons.gameObject.SetActive(value: true);
		isAtm = true;
	}

	public void openAsDonations()
	{
		donating = true;
		open();
		clear();
		amountButtons.gameObject.SetActive(value: true);
		AccountTypeTitle.text = ConversationGenerator.generate.GetJournalNameByTag("Town Debt");
		titleText.text = ConversationGenerator.generate.GetJournalNameByTag("Donate");
	}

	public void close()
	{
		window.gameObject.SetActive(value: false);
		bankOpen = false;
		difference = 0;
		amount = "";
		donating = false;
		converting = false;
		amountButtons.SetActive(value: false);
		checkBalanceForMilestones();
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void withdrawButton()
	{
		depositing = false;
		titleText.text = ConversationGenerator.generate.GetJournalNameByTag("Withdraw");
		checkBalanceForMilestones();
		clear();
	}

	public void depositButton()
	{
		depositing = true;
		titleText.text = ConversationGenerator.generate.GetJournalNameByTag("Deposit");
		checkBalanceForMilestones();
		clear();
	}

	public void convertButton()
	{
		converting = true;
		titleText.text = ConversationGenerator.generate.GetJournalNameByTag("Convert") + " [<sprite=11> 500 = <sprite=15> 1]";
		clear();
	}

	public void confirmButton()
	{
		if (converting)
		{
			confirmConversionWindow.SetActive(value: true);
			difference = Mathf.RoundToInt((float)difference / 500f) * 500;
			while (difference > accountBalance)
			{
				difference -= 500;
				if (difference <= 0)
				{
					difference = 0;
					break;
				}
			}
			dinkConversionTotal.text = difference.ToString("n0");
			permitPointConversionTotal.text = Mathf.RoundToInt((float)difference / 500f).ToString("n0");
			return;
		}
		if (donating)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.Instance.wallet);
			difference = Mathf.Clamp(difference, 0, NetworkMapSharer.Instance.townDebt);
			amountInAccount.text = NetworkMapSharer.Instance.townDebt.ToString("n0");
			amountChanging.text = difference.ToString("n0");
			NetworkMapSharer.Instance.localChar.CmdPayTownDebt(difference);
			Inventory.Instance.changeWallet(-difference, addToTownEconomy: false);
			close();
			if (NetworkMapSharer.Instance.townDebt == 0)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, TownManager.manage.debtCompleteConvo);
			}
		}
		else if (depositing)
		{
			accountBalance += difference;
			Inventory.Instance.changeWallet(-difference, addToTownEconomy: false);
			capAccountBalanceAndMoveToOverflow();
		}
		else
		{
			accountBalance -= difference;
			Inventory.Instance.changeWallet(difference, addToTownEconomy: false);
			capAccountBalanceAndMoveToOverflow();
			if (isAtm)
			{
				close();
			}
		}
		updateAccountAmounts();
		checkBalanceForMilestones();
		clear();
	}

	public void walletOverflowIntoBank(int overflow)
	{
		accountBalance += overflow;
		capAccountBalanceAndMoveToOverflow();
		NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("MoneyPocketFull"), ConversationGenerator.generate.GetNotificationText("MoneyPocketFull_Sub"));
	}

	public void capAccountBalanceAndMoveToOverflow()
	{
		if (accountBalance > billion)
		{
			accountOverflow += (ulong)((long)accountBalance - (long)billion);
			accountBalance = billion;
			return;
		}
		if (accountBalance < billion && accountOverflow != 0)
		{
			int num = billion - accountBalance;
			if (accountOverflow > (ulong)num)
			{
				accountOverflow -= (ulong)num;
				accountBalance = billion;
			}
			else
			{
				accountBalance += (int)accountOverflow;
				accountOverflow = 0uL;
			}
		}
		updateAccountAmounts();
	}

	public void confirmConversionButton()
	{
		accountBalance -= difference;
		PermitPointsManager.manage.addPoints((int)((float)difference / 500f));
		confirmConversionWindow.SetActive(value: false);
		Inventory.Instance.setAsActiveCloseButton(closeWindowButton);
		converting = false;
		updateAccountAmounts();
		checkBalanceForMilestones();
		capAccountBalanceAndMoveToOverflow();
		clear();
	}

	public void cancelConversionButton()
	{
		confirmConversionWindow.SetActive(value: false);
		Inventory.Instance.setAsActiveCloseButton(closeWindowButton);
	}

	public void cancelButton()
	{
		if (donating || isAtm)
		{
			close();
		}
		converting = false;
	}

	public void toAccountButton(int addToDifference)
	{
		amount += addToDifference;
		try
		{
			difference = int.Parse(amount);
		}
		catch
		{
			difference = accountBalance;
		}
		if (converting)
		{
			difference = Mathf.Clamp(difference, 0, accountBalance);
		}
		else if (donating)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.Instance.wallet);
			difference = Mathf.Clamp(difference, 0, NetworkMapSharer.Instance.townDebt);
		}
		else if (depositing)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.Instance.wallet);
		}
		else
		{
			difference = Mathf.Clamp(difference, 0, accountBalance);
		}
		amount = difference.ToString() ?? "";
		updateAccountAmounts();
	}

	public void clear()
	{
		amount = "";
		difference = 0;
		updateAccountAmounts();
	}

	public void max()
	{
		amount = "";
		difference = 0;
		if (donating)
		{
			toAccountButton(NetworkMapSharer.Instance.townDebt);
		}
		else
		{
			toAccountButton(billion);
		}
		updateAccountAmounts();
	}

	public void updateAccountAmounts()
	{
		if (!donating)
		{
			amountInAccount.text = ((ulong)accountBalance + accountOverflow).ToString("n0");
			amountChanging.text = difference.ToString("n0");
		}
		else
		{
			amountInAccount.text = NetworkMapSharer.Instance.townDebt.ToString("n0");
			amountChanging.text = difference.ToString("n0");
		}
	}

	public void addDailyInterest()
	{
		accountBalance += Mathf.RoundToInt((float)accountBalance / 100f * 8f / 56f);
	}

	public void checkBalanceForMilestones()
	{
		if (accountBalance >= 1000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 0)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 2000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 1)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 3000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 2)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 4000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 3)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 5000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 4)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 6000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 5)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 6000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 6)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 7000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 7)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 8000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 8)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 9000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 9)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 10000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 10)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
	}
}
