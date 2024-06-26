using System.Text.RegularExpressions;
using Pix.DTOs;
using Pix.Exceptions;
using Pix.Models;
using Pix.Repositories;

namespace Pix.Services;

public class KeyService(KeyRepository keyRepository, UserRepository userRepository, AccountRepository accountRepository, BankRepository bankRepository)
{
    private readonly KeyRepository _keyRepository = keyRepository;

    private readonly UserRepository _userRepository = userRepository;

    private readonly AccountRepository _accountRepository = accountRepository;

    private readonly BankRepository _bankRepository = bankRepository;

    public async Task<Key> CreateKey(CreateKeyDTO data, Bank bank)
    {
       
        // Verify Types
        if (data.Key.Type != "CPF" && data.Key.Type != "Phone" && data.Key.Type != "Email" && data.Key.Type != "Random") throw new InvalidTypeException("Type must be CPF, Phone, Email or Random");

        // Verify Value Equals to CPF
        if (data.Key.Type == "CPF" )
        {
            if (data.User.Cpf != data.Key.Value) throw new CpfDifferentException("The key must have the same value as the CPF");
        }

        // Verify Value Type
        var TypeIsValid = ValidateType(data.Key.Type, data.Key.Value);
        if (TypeIsValid == false) throw new InvalidFormatException("The value doesn't correspond to the type");

        // Verify if User exists
        User? user = await _userRepository.GetUserByCpf(data.User.Cpf) ?? throw new NotFoundException("User not found!");

        // Verify if the key already exists
        var availableKey = await _keyRepository.GetKeyByValue(data.Key.Value);
        if (availableKey != null) throw new UnavailableKeyException("This key already exists");

        Key newKey = new (data.Key.Value, data.Key.Type)
        {
            UserId = user.Id
        };

        // Verify total User keys
        Key[] totalKeys = await _keyRepository.CountUserKeys(user.Id);
        if (totalKeys.Length >= 20) throw new LimitExceededException("User cannot have more than 20 keys");

        // Verify total Bank User Keys
        Key[] totalKeyInThisBank = totalKeys.Where(k => k.Account.BankId.Equals(bank.Id)).ToArray();
        if (totalKeyInThisBank.Length >= 5) throw new LimitExceededException("User cannot have more than 5 keys in this Bank");

        // Verify if there is an account
        Account? account = await _accountRepository.GetAccountByNumandBank(data.Account.Number, bank.Id);  
        if (account == null) {
            Account newAccount = new(data.Account.Agency, data.Account.Number)
            {
                UserId = user.Id,
                BankId = bank.Id,
            };

            await _accountRepository.CreateAccount(newAccount);
            Console.WriteLine("New Account Created");

            newKey.AccountId = newAccount.Id;
        }else{
            newKey.AccountId = account.Id;
        }

        return await _keyRepository.Createkey(newKey);
    }

    private static bool ValidateType(string type, string value)
    {
        switch (type)
        {
            case "CPF":
                return value.Length == 11 && long.TryParse(value, out _);
            case "Phone":
                return value.Length >=8 && value.Length <=11 && long.TryParse(value, out _);
            case "Email":
                string emailPattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";
                return Regex.IsMatch(value, emailPattern);
            case "Random":
                return true;
            default:
                return false;
        }
    }

    public async Task<KeyInfoDto?> GetKeyInfo(string type, string value)
    {
        Key? key = await _keyRepository.GetKeyByTypeAndValue(type, value) ?? throw new NotFoundException("Key not found!");

        User? user = await _userRepository.GetUserById(key.UserId) ?? throw new NotFoundException("User not found!");

        Account? account = await _accountRepository.GetAccountById(key.AccountId) ?? throw new NotFoundException("Account not found!");

        Bank? bank = await _bankRepository.GetBankById(account.BankId) ?? throw new NotFoundException("Bank not found!");

        KeyInfoDto keyInfo = new()
        {
            Key = new KeyDto
            {
                Value = key.Value,
                Type = key.Type
            },
            User = new UserDto
            {
                Name = user.Name,
                MaskedCpf = MaskCpf(user.Cpf)
            },
            Account = new AccountDto
            {
                Number = account.Number,
                Agency = account.Agency,
                BankName = bank.Name,
                BankId = bank.Id
            }
        };

        return keyInfo;
    }

    private static string MaskCpf(string cpf)
    {
        return cpf[..3] + ".***.***-" + cpf[^2..];
    }
}