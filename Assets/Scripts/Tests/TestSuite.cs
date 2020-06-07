using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestSuite
{
    GameObject playerObject;
    private Player player;
    private Controller2D controller;

    [SetUp]
    public void Setup()
    {
        playerObject = GameObject.Instantiate(new GameObject());
        LayerMask playerLayerMask = LayerMask.NameToLayer("Player");
        LayerMask collisionLayerMask = LayerMask.NameToLayer("Obstacle");
        playerObject.layer = playerLayerMask.value;
        controller = Controller2D.CreateController(playerObject, collisionLayerMask);
        player = Player.CreatePlayer(playerObject, controller);
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
    }

    [UnityTest]
    public IEnumerator PlayerFallsDown()
    {
        float initialYPos = playerObject.transform.position.y;
        yield return new WaitForSeconds(1f);

        Assert.Less(playerObject.transform.position.y, initialYPos);
    }

    //[UnityTest]
    //public IEnumerator PlayerFallsDown()
    //{
    //    float initialYPos = playerObject.transform.position.y;
    //    yield return new WaitForSeconds(1f);

    //    Assert.Less(playerObject.transform.position.y, initialYPos);
    //}
}