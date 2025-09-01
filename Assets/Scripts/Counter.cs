using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using InzGame.DisplayHandlers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Counter : MonoBehaviour {
    public int size = 4;
    public int gatekeepSize = 2;
    public int numCustomers = 0;
    public List<GameObject> anchors;
    public GameObject customerPrefab;

    public List<Sprite> maleSprites;
    public List<Sprite> femaleSprites;

    public CustomerInstance[] customers;
    public GameConfiguration _config;

    public GameManager _gameManager;

    private void Awake() {
        _gameManager = GetComponent<GameManager>();
        _config = _gameManager.config;
        customers = new CustomerInstance[size];

        foreach (var anchor in anchors) {
            anchor.gameObject.SetActive(false);
        }
    }

    public bool CanAdd() {
        return gatekeepSize > 0
            ? numCustomers < gatekeepSize
            : numCustomers < size;
    }

    public void AddCustomer(CustomerPreset preset) {
        int index = FindFreeSeat();
        var customer = Instantiate(customerPrefab, anchors[ index ].transform);

        var customerInstance = customer.GetComponent<CustomerInstance>();
        customerInstance.OnCustomerRemove += RemoveCustomer;
        customerInstance.preset = preset;
        customerInstance.seat = index;

        var customerDisplay = customer.GetComponent<CustomerDisplay>();
        customerDisplay.personSprites = Random.Range(0, 2) == 0
            ? maleSprites
            : femaleSprites;

        customerDisplay.InitImages();
        // customerDisplay.personImage.sprite = customerDisplay.personSprites[0];
        // customerDisplay.personImageNext_Dg.sprite = customerDisplay.personSprites[1];

        customerDisplay.orderImage.sprite = _config.elementProperties.GetFor(preset.order).sprite_order_bubble;
        customers[index] = customerInstance;
        numCustomers++;
        customer.transform.parent.gameObject.SetActive(true);

        Debug.Log("Customer added; numCustomers: " + numCustomers);
    }

    public void RemoveCustomer(CustomerInstance customer) {
        int idx = Array.IndexOf(customers, customer);
        customers[idx] = null;
        numCustomers--;

        anchors[idx].gameObject.SetActive(false);

        Debug.Log("Customer removed; numCustomers: " + numCustomers);
    }

    public int FindFreeSeat(bool randomized = false) {
        List<int> indices = new List<int>(size);
        for (int i = 0; i < size; i++) {
            indices.Add(i);
        }
        if (randomized)
            indices = indices.OrderBy(_ => Random.value).ToList();

        for (int i = 0; i < size; i++) {
            if (customers[indices[i]] == null) {
                return indices[i];
            }
        }
        return -1;
    }

    public void RemoveCustomer(object sender, EventArgs _) {
        RemoveCustomer((CustomerInstance) sender);
    }
}
