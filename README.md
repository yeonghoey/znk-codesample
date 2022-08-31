# <좀비와 열쇠> 코드 샘플
![Cover.png](/Images/Cover.png)

이 코드 샘플은 <[좀비와 열쇠(Zombies and Keys)](https://store.steampowered.com/app/1167150/Zombies_and_Keys/)>의 구현 방식을 설명하기 위한 플레이어 캐릭터 관련 코드를 담고 있습니다. 전부 개발자가 직접 작성한 코드이며, 실제 게임에 사용된 코드이지만 저장소에 포함된 코드만으로는 실행되지 않습니다. 아래에 코드의 이해를 돕기 위한 설명이 이어집니다.

## MessageExchange
몇 번의 습작을 개발하는 과정에서 컴포넌트 간의 책임을 잘 분리하지 않아 몇몇 MonoBehaviour가 과도한 책임을 맡게 되어 비대해지는 상황을 많이 겪었습니다. 비대해진 MonoBehaviour는 버그가 발생하기 쉬우며 기능 추가 및 수정이 어려웠습니다.

![MessageExchange.png](/Images/MessageExchange.png)

이 문제를 해결하기 위해 <좀비와 열쇠>에서는 각 기능을 독립적으로 구현할 수 있도록 [MessageExchange.cs]라는 간단한 메시징(이벤트) 시스템을 구현했습니다. 게임 속 개체(플레이어, 좀비 등)를 이루는 컴포넌트들은 이 시스템을 기반으로 상호작용합니다.

MessageExchange는 Meditator 디자인 패턴이자 PubSub 패턴으로 볼 수 있고, 그 구현 방식은 특정 interface를 구현한 MonoBehaviour가 해당 메시지를 처리하도록 하는, Unity UI의 [Messaging System]을 단일 GameObject가 아닌, 단일 개체의 전체, 즉 GameObject Hierarchy로 확장한 버전으로 생각할 수 있습니다.

MessageExchange를 위한 interface는 모두 `IMessageExchangeTarget`을 상속하며, 그 이름은 `IMET`로 시작합니다. 예를 들어 플레이어의 체력이 변동된 경우, 플레이어의 체력을 담당하는 [PlayerHealth.cs]는 다음과 같이 MessageExchange 메시지를 발생시킵니다.

```csharp
public interface IMETPlayerHealthOnChanage : IMessageExchangeTarget
{
    void OnChanage(int healthRemaining);
}

// NOTE: Garbage가 생성되는 것을 막기 위해 readonly로 만들어 둡니다.
private static readonly Action<IMETPlayerHealthOnChanage, int> callOnChange =
    (t, healthRemaining) => t.OnChanage(healthRemaining);

messageExchange.Invoke(callOnChange, health);
```

플레이어의 체력이 1 남은 경우, 체력표시 HUD를 빨갛게 깜빡이는데요, HUD 기능을 제어하는 [PlayerHUDDriver.cs]는 `IMETPlayerHealthOnChanage`를 다음과 같이 처리합니다.

```csharp
// 관련 내용을 제외하고 모두 생략했습니다.
public class PlayerHUDDriver : MonoBehaviour,
    IMETPlayerHealthOnChanage
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private HUDDotContainer hudHealth;

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerHealthOnChanage.OnChanage(int healthRemaining)
    {
        hudHealth.IsBlinking = healthRemaining == 1;
    }
}
```


## Animator와 MessageExchange 연동
<좀비와 열쇠>는 게이머에게 보이는 것과 게임 로직이 매끄럽게 연결되는 반응성 좋은 액션 게임이 되길 원했습니다. 이 목표를 달성하기 위해 Animator의 State와 State 간 트랜지션에 따라 게임 로직이 반응하도록 게임을 설계했습니다.

![AnimatorController.png](/Images/AnimatorController.png)

Animator 내부에서 일어나는 모든 일을 MessageExchange를 거치도록 하기 위해 Unity Animator의 콜백 이벤트들을 연결해주는 어댑터 컴포넌트들을 작성했습니다.

![MessageExchange-Animator.png](/Images/MessageExchange-Animator.png)

[PlayerSMBIdle.cs]과 같이 `PlayerSMB`로 시작하는 파일들은 Animator의 각 State 대응되는 `StateMachineBehaviour`를 코드이며 해당 상태의 Enter, Exit 등의 이벤트를 MessageExchange 메시지로 변환시키는 역할을 합니다.

Animator가 설치된 GameObject의 Component의 함수 이름으로 호출하는 레거시 방식의 [Animation Event]의 경우에는 [PlayerAnimationEventProxy.cs]에서 [Animation Event] 호출을 MessageExchange 메시지로 변환합니다.


## MessageInterchange
MessageExchange의 전역 버전으로 [MessageInterchange.cs]도 있습니다. 정확히 똑같이 동작하지만 ScriptableObject 형태로 구현되어 개체 간의 통신을 담당합니다. 그 한 가지 예는 "입력처리"입니다.

![MessageInterchange.png](/Images/MessageInterchange.png)

게이머의 조작 자체는 게임상에서 유일하고, 플레이어 캐릭터와는 직접적인 상관이 없기 때문에, 별도의 개체에 구현했습니다. [PlayerInputDriver.cs]는 Unity의 New Input System을 콜백을 다음과 같은 MessageInterchange 메시지로 변환하여 발생시킵니다. MessageExchange와 마찬가지로 MessageInterchange를 거치는 메시지 interface는 `IMIT`로 시작합니다.

```csharp
public interface IMITPlayerInputDriver : IMessageInterchangeTarget
{
    void OnMove(Vector2 inputDir);
    void OnAttack(Vector2 inputDir);
    void OnRoll(bool isPressing);
    void OnTake(bool isPressing);
    void OnDrop(bool isPressing);
}
```

Player 개체에 속한 [PlayerControl.cs]은 `IMITPlayerInputDriver` 메시지를 받아 플레이어 캐릭터의 도메인 입력으로 변환하고, 그 최신 값을 다른 컴포넌트가 참조할 수 있도록 캐싱하고 있다가 제공합니다.


## Actor와 FSM

![Actor.png](/Images/Actor.png)

[PlayerActorIdle.cs]와 같이 `PlayerActor`로 시작하는 파일들은 [PlayerControl.cs]에서 입력을 읽고 MessageExchange에서 적절한 메시지를 읽어 간단한 유한 상태 기계([FiniteStateMachine.cs]) 로직에 따라 상태를 바꿔가며 동작하는, 도메인 로직이 구현된 파일입니다.

# 기타
- [Player.cs] - 피격 처리 등, 현재 플레이어 개체가 외부 특정 개체와 직접 상호작용 역할을 할 때 필요한 기능이 구현되어 있습니다.
- [PlayerParamData.cs] - 플레이어가 맨손 일때와 표지판을 들었을 때의 파라매터 데이터를 담고 있는 ScriptableObject입니다.
- [Locomotor.cs] - 플레이어 캐릭터와 좀비 공통으로 사용되는 이동 입력 보정같은 물리 상호작용에 사용되는 로직
- [BasicTypeExtensions.cs] - [Extension Method]로 구현된 자주 사용되는 유틸리티 기능들


[MessageExchange.cs]: /Core/MessageExchange.cs
[MessageInterchange.cs]: /Core/MessageInterchange.cs
[FiniteStateMachine.cs]: /Core/FiniteStateMachine.cs
[BasicTypeExtensions.cs]: /Core/BasicTypeExtensions.cs
[Locomotor.cs]: /Core/Locomotor.cs

[Player.cs]: /Player/Player.cs
[PlayerParamData.cs]: /Player/PlayerParamData.cs
[PlayerInputDriver.cs]: /Input/PlayerInputDriver.cs
[PlayerControl.cs]: /Player/PlayerControl.cs
[PlayerHealth.cs]: /Player/PlayerHealth.cs
[PlayerHUDDriver.cs]: /Player/PlayerHUDDriver.cs
[PlayerInputDriver.cs]: /Player/PlayerInputDriver.cs
[PlayerSMBIdle.cs]: /Player/PlayerSMBIdle.cs
[PlayerAnimationEventProxy.cs]: /Player/PlayerAnimationEventProxy.cs
[PlayerActorIdle.cs]: /Player/PlayerActorIdle.cs

[Messaging System]: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/MessagingSystem.html
[Animation Event]: https://docs.unity3d.com/Manual/script-AnimationWindowEvent.html
[Extension Method]: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
