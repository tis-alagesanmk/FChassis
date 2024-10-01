using System.Windows;
namespace FChassis;
using static MCSettings.EHeads;

/// <summary>Interaction logic for SettingsDlg.xaml</summary>
public partial class SettingsDlg : Window {
   public SettingsDlg () {
      InitializeComponent ();
      var set = MCSettings.It;
      tbStandoff.Bind (() => set.Standoff, f => set.Standoff = f.Clamp (0, 100));
      tbPartition.Bind (() => set.PartitionRatio, f => set.PartitionRatio = f.Clamp (0, 1));
      tbStepLength.Bind (() => set.StepLength, f => set.StepLength = f.Clamp (0.05, 10));
      cbPingPong.Bind (() => set.UsePingPong, b => set.UsePingPong = b);
      cbOptimize.Bind (() => set.OptimizePartition, b => set.OptimizePartition = b);
      tbMarkText.Bind (() => set.MarkText, s => set.MarkText = s);
      tbMarkTextPositionX.Bind (() => set.MarkTextPosX, f => set.MarkTextPosX = f.Clamp (0.05, 100000));
      tbMarkTextPositionY.Bind (() => set.MarkTextPosY, f => set.MarkTextPosY = f.Clamp (0.05, 100000));
      lbPriority.Bind (btnPrioUp, btnPrioDown, () => set.ToolingPriority, a => set.ToolingPriority = [..a.OfType<EKind> ()]);
      rbBoth.Bind (() => set.Heads == Both, () => set.Heads = Both);
      rbLeft.Bind (() => set.Heads == MCSettings.EHeads.Left, () => set.Heads = MCSettings.EHeads.Left);
      rbRight.Bind (() => set.Heads == Right, () => set.Heads = Right);
      rbLHComponent.Bind (() => set.PartConfig == MCSettings.PartConfigType.LHComponent, () => set.PartConfig = MCSettings.PartConfigType.LHComponent);
      rbRHComponent.Bind (() => set.PartConfig == MCSettings.PartConfigType.RHComponent, () => set.PartConfig = MCSettings.PartConfigType.RHComponent);
      tbApproachLength.Bind (() => set.ApproachLength, al => set.ApproachLength = al.Clamp (0, 6));
      tbNotchApproachLength.Bind (() => set.NotchApproachLength, al => set.NotchApproachLength = al.Clamp (0, 6));
      tbNotchWireJointDistance.Bind (() => set.NotchWireJointDistance, al => set.NotchWireJointDistance = al.Clamp (0, 6));
      btnOK.Bind (Close);
   }
}
