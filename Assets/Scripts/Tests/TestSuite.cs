using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestSuite
{
    GameObject playerObject;
    private Player player;
    private Controller2D controller;
    private Scene testScene;
    private IUnityService unityService;

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerObject);
    }

    public IEnumerator SetupScene()
    {
        testScene = SceneManager.GetActiveScene();
        yield return SceneManager.LoadSceneAsync("TestScene", LoadSceneMode.Additive);
    }

    public void SetupPlayer()
    {
        playerObject = GameObject.Find("Player");
        player = playerObject.GetComponent<Player>();
        unityService = Substitute.For<IUnityService>();
        unityService.GetFixedDeltaTime().Returns(0.04f);
        player.UnityService = unityService;

        controller = playerObject.GetComponent<Controller2D>();
    }

    [UnityTest]
    public IEnumerator TestSceneLoading()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // ---------------ASSERT---------------
        Assert.IsTrue(SceneManager.GetActiveScene().name == "TestScene");

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Moves_Right_With_Input()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Position
        float initialXPos = playerObject.transform.position.x;

        // Mock input
        unityService.GetAxisRaw("Horizontal").Returns(1);

        yield return null;

        Assert.Greater(playerObject.transform.position.x, initialXPos);
        Debug.Log("Distance moved: " + (playerObject.transform.position.x - initialXPos));

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Moves_Left_With_Input()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Position
        float initialXPos = playerObject.transform.position.x;

        // Mock
        unityService.GetAxisRaw("Horizontal").Returns(-1);

        yield return null;

        Assert.Less(playerObject.transform.position.x, initialXPos);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator PlayerFallsDown()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        float initialYPos = playerObject.transform.position.y;

        yield return new WaitForSeconds(1f);

        // ---------------ASSERT---------------
        Assert.Less(playerObject.transform.position.y, initialYPos);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }


    [UnityTest]
    public IEnumerator Collides_With_Ground()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Set position above flat ground
        GameObject ground = GameObject.Find("FlatGround 1");
        playerObject.transform.position = new Vector3(ground.transform.position.x, ground.transform.position.y + ground.transform.localScale.y, 0);

        // Fall
        yield return null;

        // ---------------ASSERT---------------
        Assert.True(controller.Collisions.below);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Climbs_Slope_At_Regular_Speed()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Mock
        unityService.GetAxisRaw("Horizontal").Returns(-1);

        // Flat 
        GameObject ground = GameObject.Find("FlatGround 1");
        playerObject.transform.position = new Vector3(ground.transform.position.x + playerObject.transform.localScale.x, ground.transform.position.y + playerObject.transform.localScale.y, 0);
        float initialPosXFlat = playerObject.transform.position.x;

        // Tick
        yield return null;

        float deltaPosXFlat = initialPosXFlat - playerObject.transform.position.x;

        // Slope
        GameObject slope = GameObject.Find("NegSlope 1");
        playerObject.transform.position = new Vector3(slope.transform.position.x + playerObject.transform.localScale.x -0.4f, slope.transform.position.y + playerObject.transform.localScale.y, 0);
        float initialPosXSlope = playerObject.transform.position.x;

        // Tick
        yield return null;

        float deltaPosXSlope = initialPosXSlope - playerObject.transform.position.x;

        Debug.Log("initialPosXFlat: " + initialPosXFlat);
        Debug.Log("playerx: " + playerObject.transform.position.x);
        Debug.Log("deltaPosXFlat: " + deltaPosXFlat);

        Debug.Log("initialPosXSlope: " + initialPosXSlope);
        Debug.Log("playerx: " + playerObject.transform.position.x);
        Debug.Log("deltaPosXSlope: " + deltaPosXSlope);

        // ---------------ASSERT---------------
        Assert.IsTrue(controller.Collisions.climbingSlope);
        Assert.IsTrue(controller.Collisions.below); // Allows jump
        Assert.IsFalse(controller.Collisions.left);
        Assert.AreEqual(deltaPosXFlat, deltaPosXSlope, 0.2f);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Collides_Right_With_Block()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        GameObject blockRight = GameObject.Find("BlockRight");
        playerObject.transform.position = new Vector3(blockRight.transform.position.x - blockRight.transform.localScale.x, blockRight.transform.position.y, 0);


        yield return null;

        // ---------------ASSERT---------------
        Assert.False(controller.Collisions.right);

        // Mock input
        unityService.GetAxisRaw("Horizontal").Returns(1);

        yield return new WaitForSeconds(0.5f);

        // ---------------ASSERT---------------
        Assert.True(controller.Collisions.right);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Collides_Left_With_Block()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        GameObject blockLeft = GameObject.Find("BlockLeft");
        playerObject.transform.position = new Vector3(blockLeft.transform.position.x + blockLeft.transform.localScale.x, blockLeft.transform.position.y, 0);


        yield return null;

        // ---------------ASSERT---------------
        Assert.False(controller.Collisions.left);

        // Mock input
        unityService.GetAxisRaw("Horizontal").Returns(-1);

        yield return new WaitForSeconds(0.5f);

        // ---------------ASSERT---------------
        Assert.True(controller.Collisions.left);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Jumps_With_Input()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Set position on flat ground
        GameObject ground = GameObject.Find("FlatGround 1");
        playerObject.transform.position = new Vector3(ground.transform.position.x, ground.transform.position.y + ground.transform.localScale.y, 0);

        yield return null;

        // ---------------ASSERT---------------
        Assert.True(controller.Collisions.below);

        float initialYPos = playerObject.transform.position.y;

        // Mock input
        unityService.GetKeyDown(KeyCode.Space).Returns(true);

        yield return new WaitForSeconds(0.5f);

        // ---------------ASSERT---------------
        Assert.False(controller.Collisions.below);
        Assert.Greater(playerObject.transform.position.y, initialYPos);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }

    [UnityTest]
    public IEnumerator Collides_Top()
    {
        // ---------------SET UP---------------
        yield return SetupScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));

        // Player
        SetupPlayer();

        // Set position below flat ground
        GameObject ground = GameObject.Find("FlatGround 2");


        yield return null;

        // ---------------ASSERT---------------
        Assert.False(controller.Collisions.above);

        playerObject.transform.position = new Vector3(ground.transform.position.x, ground.transform.position.y - ground.transform.localScale.y -0.5f, 0);

        Debug.Log("Player position: " + playerObject.transform.position.x + ", " + playerObject.transform.position.y);

        // Mock velocity
        player.movement.ReachedApex = false;

        while (controller.Collisions.above == false)
        {
            player.movement.Velocity = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 20f);
            yield return null;
        }

        Debug.Log("Player position: " + playerObject.transform.position.x + ", " + playerObject.transform.position.y);

        // ---------------ASSERT---------------
        Assert.False(controller.Collisions.left);
        Assert.False(controller.Collisions.right);
        Assert.False(controller.Collisions.below);
        Assert.True(controller.Collisions.above);

        // --------------CLEAN UP--------------
        SceneManager.SetActiveScene(testScene);
        yield return SceneManager.UnloadSceneAsync("TestScene");
    }
}