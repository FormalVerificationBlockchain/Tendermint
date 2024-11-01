using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PAT.Common.Classes.Expressions.ExpressionClass;

//the namespace must be PAT.Lib, the class and method names can be arbitrary
namespace PAT.Lib {
    /// <summary>
    /// The math library that can be used in your model.
    /// all methods should be declared as public static.
    /// 
    /// The parameters must be of type "int", or "int array"
    /// The number of parameters can be 0 or many
    /// 
    /// The return type can be bool, int or int[] only.
    /// 
    /// The method name will be used directly in your model.
    /// e.g. call(max, 10, 2), call(dominate, 3, 2), call(amax, [1,3,5]),
    /// 
    /// Note: method names are case sensetive
    /// </summary>
    public class Message : ExpressionValue {
        public int processId;
        public int round;
        public int value;

        public Message() {
            this.processId = -1;
            this.round = -1;
            this.value = -1;
        }

        public int GetProcessId() {
            return this.processId;
        }

        public int GetRound() {
            return this.round;
        }

        public int GetValue() {
            return this.value;
        }
    }

    public class ProposalMessage : Message {
        public int validRound;

        public ProposalMessage() : base() {
            this.validRound = -1;
        }

        public ProposalMessage(int processId, int round, int value, int validRound) {
            this.processId = processId;
            this.round = round;
            this.value = value;
            this.validRound = validRound;
        }

        public int GetValidRound() {
            return this.validRound;
        }

        public override string ToString() {
            return ExpressionID;
        }

        public override ExpressionValue GetClone() {
            return new ProposalMessage(this.processId, this.round, this.value, this.validRound);
        }

        public override string ExpressionID {
            get {
                return "(PROPOSAL, round=" + this.round + ", v=" + this.value + ", vr=" + this.validRound + ") by PROCESS " + this.processId;
            }
        }
    }

    public class PrevoteMessage : Message {
        public PrevoteMessage() : base() {}

        public PrevoteMessage(int processId, int round, int value) {
            this.processId = processId;
            this.round = round;
            this.value = value;
        }

        public override string ToString() {
            return ExpressionID;
        }

        public override ExpressionValue GetClone() {
            return new PrevoteMessage(this.processId, this.round, this.value);
        }

        public override string ExpressionID {
            get {
                return "(PREVOTE, round=" + this.round + ", v=" + this.value + ") by PROCESS " + this.processId;
            }
        }
    }

    public class PrecommitMessage : Message {
        public PrecommitMessage() : base() {}

        public PrecommitMessage(int processId, int round, int value) {
            this.processId = processId;
            this.round = round;
            this.value = value;
        }

        public override string ToString() {
            return ExpressionID;
        }

        public override ExpressionValue GetClone() {
            return new PrecommitMessage(this.processId, this.round, this.value);
        }

        public override string ExpressionID {
            get {
                return "(PRECOMMIT, round=" + this.round + ", v=" + this.value + ") by PROCESS " + this.processId;
            }
        }
    }

    public class MessageLog : ExpressionValue {
        public Dictionary<int, ProposalMessage> proposals;
        public Dictionary<int, List<PrevoteMessage>> prevotes;
        public Dictionary<int, List<PrecommitMessage>> precommits;
        public Dictionary<int, int> roundMessageCounts;
        public Dictionary<int, int> roundPrevoteMessageCounts;
        public Dictionary<int, int> roundPrecommitMessageCounts;
        public Dictionary<int, Dictionary<int, int>> roundPrevotedValueCounts;
        public Dictionary<int, Dictionary<int, int>> roundPrecommittedValueCounts;
        public Dictionary<int, int> roundCommitCandidates;
        public int f;
        public int threshold;
        public int latestRoundWithSufficientMessages;
        public int commitReadyValue;
        public const int DefaultValue = -2;

        public MessageLog(int f) {
            this.f = f;
            this.threshold = 2 * f + 1;
            this.proposals = new Dictionary<int, ProposalMessage>();
            this.prevotes = new Dictionary<int, List<PrevoteMessage>>();
            this.precommits = new Dictionary<int, List<PrecommitMessage>>();
            this.roundMessageCounts = new Dictionary<int, int>();
            this.roundPrevoteMessageCounts = new Dictionary<int, int>();
            this.roundPrecommitMessageCounts = new Dictionary<int, int>();
            this.roundPrevotedValueCounts = new Dictionary<int, Dictionary<int, int>>();
            this.roundPrecommittedValueCounts = new Dictionary<int, Dictionary<int, int>>();
            this.roundCommitCandidates = new Dictionary<int, int>();
            this.latestRoundWithSufficientMessages = -1;
            this.commitReadyValue = MessageLog.DefaultValue;
        }

        public MessageLog(MessageLog messageLog) {
            this.proposals = new Dictionary<int, ProposalMessage>();
            foreach (KeyValuePair<int, ProposalMessage> entry in messageLog.proposals) {
                this.proposals[entry.Key] = new ProposalMessage(entry.Value.processId, entry.Value.round, entry.Value.value, entry.Value.validRound);
            }
            this.prevotes = new Dictionary<int, List<PrevoteMessage>>();
            foreach (KeyValuePair<int, List<PrevoteMessage>> entry in messageLog.prevotes) {
                List<PrevoteMessage> prevoteMessages = new List<PrevoteMessage>();
                foreach (PrevoteMessage prevoteMessage in entry.Value) {
                    prevoteMessages.Add(new PrevoteMessage(prevoteMessage.processId, prevoteMessage.round, prevoteMessage.value));
                }
                this.prevotes[entry.Key] = prevoteMessages;
            }
            this.precommits = new Dictionary<int, List<PrecommitMessage>>();
            foreach (KeyValuePair<int, List<PrecommitMessage>> entry in messageLog.precommits) {
                List<PrecommitMessage> precommitMessages = new List<PrecommitMessage>();
                foreach (PrecommitMessage precommitMessage in entry.Value) {
                    precommitMessages.Add(new PrecommitMessage(precommitMessage.processId, precommitMessage.round, precommitMessage.value));
                }
                this.precommits[entry.Key] = precommitMessages;
            }
            this.roundMessageCounts = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> entry in messageLog.roundMessageCounts) {
                this.roundMessageCounts[entry.Key] = entry.Value;
            }
            this.roundPrevoteMessageCounts = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> entry in messageLog.roundPrevoteMessageCounts) {
                this.roundPrevoteMessageCounts[entry.Key] = entry.Value;
            }
            this.roundPrecommitMessageCounts = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> entry in messageLog.roundPrecommitMessageCounts) {
                this.roundPrecommitMessageCounts[entry.Key] = entry.Value;
            }
            this.roundPrevotedValueCounts = new Dictionary<int, Dictionary<int, int>>();
            foreach (KeyValuePair<int, Dictionary<int, int>> outerEntry in messageLog.roundPrevotedValueCounts) {
                Dictionary<int, int> prevotedValueCounts = new Dictionary<int, int>();
                foreach (KeyValuePair<int, int> innerEntry in outerEntry.Value) {
                    prevotedValueCounts[innerEntry.Key] = innerEntry.Value;
                }
                this.roundPrevotedValueCounts[outerEntry.Key] = prevotedValueCounts;
            }
            this.roundPrecommittedValueCounts = new Dictionary<int, Dictionary<int, int>>();
            foreach (KeyValuePair<int, Dictionary<int, int>> outerEntry in messageLog.roundPrecommittedValueCounts) {
                Dictionary<int, int> precommittedValueCounts = new Dictionary<int, int>();
                foreach (KeyValuePair<int, int> innerEntry in outerEntry.Value) {
                    precommittedValueCounts[innerEntry.Key] = innerEntry.Value;
                }
                this.roundPrecommittedValueCounts[outerEntry.Key] = precommittedValueCounts;
            }
            this.roundCommitCandidates = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> entry in messageLog.roundCommitCandidates) {
                this.roundCommitCandidates[entry.Key] = entry.Value;
            }
            this.f = messageLog.f;
            this.threshold = messageLog.threshold;
            this.latestRoundWithSufficientMessages = messageLog.latestRoundWithSufficientMessages;
            this.commitReadyValue = messageLog.commitReadyValue;
        }

        public void AddProposal(ProposalMessage proposal) {
            this.proposals[proposal.round] = proposal;
            if (!this.roundMessageCounts.ContainsKey(proposal.round)) {
                this.roundMessageCounts[proposal.round] = 1;
            } else {
                this.roundMessageCounts[proposal.round]++;
            }
            if (this.roundMessageCounts[proposal.round] > this.f && proposal.round > this.latestRoundWithSufficientMessages) {
                this.latestRoundWithSufficientMessages = proposal.round;
            }
        }

        public void AddPrevote(PrevoteMessage prevote) {
            if (!this.prevotes.ContainsKey(prevote.round)) {
                this.prevotes[prevote.round] = new List<PrevoteMessage>();
            }
            this.prevotes[prevote.round].Add(prevote);
            if (!this.roundMessageCounts.ContainsKey(prevote.round)) {
                this.roundMessageCounts[prevote.round] = 1;
            } else {
                this.roundMessageCounts[prevote.round]++;
            }
            if (this.roundMessageCounts[prevote.round] > this.f && prevote.round > this.latestRoundWithSufficientMessages) {
                this.latestRoundWithSufficientMessages = prevote.round;
            }
            if (!this.roundPrevoteMessageCounts.ContainsKey(prevote.round)) {
                this.roundPrevoteMessageCounts[prevote.round] = 1;
            } else {
                this.roundPrevoteMessageCounts[prevote.round]++;
            }
            if (!this.roundPrevotedValueCounts.ContainsKey(prevote.round)) {
                this.roundPrevotedValueCounts[prevote.round] = new Dictionary<int, int>();
                this.roundPrevotedValueCounts[prevote.round][prevote.value] = 1;
            } else {
                if (!this.roundPrevotedValueCounts[prevote.round].ContainsKey(prevote.value)) {
                    this.roundPrevotedValueCounts[prevote.round][prevote.value] = 1;
                } else {
                    this.roundPrevotedValueCounts[prevote.round][prevote.value]++;
                }
            }
        }

        public void AddPrecommit(PrecommitMessage precommit) {
            if (!this.precommits.ContainsKey(precommit.round)) {
                this.precommits[precommit.round] = new List<PrecommitMessage>();
            }
            this.precommits[precommit.round].Add(precommit);
            if (!this.roundMessageCounts.ContainsKey(precommit.round)) {
                this.roundMessageCounts[precommit.round] = 1;
            } else {
                this.roundMessageCounts[precommit.round]++;
            }
            if (this.roundMessageCounts[precommit.round] > this.f && precommit.round > this.latestRoundWithSufficientMessages) {
                this.latestRoundWithSufficientMessages = precommit.round;
            }
            if (!this.roundPrecommitMessageCounts.ContainsKey(precommit.round)) {
                this.roundPrecommitMessageCounts[precommit.round] = 1;
            } else {
                this.roundPrecommitMessageCounts[precommit.round]++;
            }
            if (!this.roundPrecommittedValueCounts.ContainsKey(precommit.round)) {
                this.roundPrecommittedValueCounts[precommit.round] = new Dictionary<int, int>();
                this.roundPrecommittedValueCounts[precommit.round][precommit.value] = 1;
            } else {
                if (!this.roundPrecommittedValueCounts[precommit.round].ContainsKey(precommit.value)) {
                    this.roundPrecommittedValueCounts[precommit.round][precommit.value] = 1;
                } else {
                    this.roundPrecommittedValueCounts[precommit.round][precommit.value]++;
                }
            }
            if (this.roundPrecommittedValueCounts[precommit.round][precommit.value] >= this.threshold) {
                this.roundCommitCandidates[precommit.round] = precommit.value;
            }
        }
        
        public int GetProposalValue(int round) {
            return this.proposals[round].GetValue();
        }

        public int GetProposalValidRound(int round) {
            return this.proposals[round].GetValidRound();
        }

        public void Clear() {
            this.proposals.Clear();
            this.prevotes.Clear();
            this.precommits.Clear();
            this.roundMessageCounts.Clear();
            this.roundPrevoteMessageCounts.Clear();
            this.roundPrecommitMessageCounts.Clear();
            this.roundPrevotedValueCounts.Clear();
            this.roundPrecommittedValueCounts.Clear();
            this.roundCommitCandidates.Clear();
            this.latestRoundWithSufficientMessages = -1;
            this.commitReadyValue = MessageLog.DefaultValue;
        }

        public bool ContainsProposal(int round, int validRound) {
            return this.proposals.ContainsKey(round) && 
                    (validRound == MessageLog.DefaultValue || this.proposals[round].GetValidRound() == validRound);
        }

        public bool ContainsSufficientPrevotes(int round, int value) {
            if (value == MessageLog.DefaultValue) {
                return this.roundPrevoteMessageCounts.ContainsKey(round) && 
                        this.roundPrevoteMessageCounts[round] >= this.threshold;
            } else {
                return this.roundPrevotedValueCounts.ContainsKey(round) && 
                        this.roundPrevotedValueCounts[round].ContainsKey(value) && 
                        this.roundPrevotedValueCounts[round][value] >= this.threshold;
            }
        }

        public bool ContainsSufficientPrecommits(int round, int value) {
            if (value == MessageLog.DefaultValue) {
                return this.roundPrecommitMessageCounts.ContainsKey(round) && 
                        this.roundPrecommitMessageCounts[round] >= this.threshold;
            } else {
                return this.roundPrecommittedValueCounts.ContainsKey(round) && 
                        this.roundPrecommittedValueCounts[round].ContainsKey(value) && 
                        this.roundPrecommittedValueCounts[round][value] >= this.threshold;
            }
        }

        public bool ContainsProposalAndSufficientPrevotesForPrevoting(int round) {
            return this.proposals.ContainsKey(round) && 
                    this.proposals[round].GetValidRound() >= 0 && 
                    this.proposals[round].GetValidRound() < round && 
                    this.ContainsSufficientPrevotes(this.proposals[round].GetValidRound(), this.proposals[round].GetValue());
        }

        public bool ContainsProposalAndSufficientPrevotesForPrecommitting(int round) {
            return this.proposals.ContainsKey(round) && 
                    this.ContainsSufficientPrevotes(round, this.proposals[round].GetValue());
        }

        public bool ContainsProposalAndSufficientPrecommits() {
            foreach (KeyValuePair<int, int> entry in this.roundCommitCandidates) {
                if (this.proposals.ContainsKey(entry.Key) && this.proposals[entry.Key].value == entry.Value) {
                    this.commitReadyValue = entry.Value;
                    return true;
                }
            }
            return false;
        }

        public bool ContainsProposalAndSufficientAdversePrecommits(int round) {
            return this.proposals.ContainsKey(round) && 
                    this.roundPrecommitMessageCounts.ContainsKey(round) && 
                    (
                        this.roundPrecommitMessageCounts[round] - 
                        (
                            this.roundPrecommittedValueCounts[round].ContainsKey(this.proposals[round].GetValue()) ? 
                            this.roundPrecommittedValueCounts[round][this.proposals[round].GetValue()] : 
                            0
                        ) 
                        > this.f
                    );
        }

        public int GetLatestRoundWithSufficientMessages() {
            return this.latestRoundWithSufficientMessages;
        }

        public int GetCommitReadyValue() {
            return this.commitReadyValue;
        }
        
        /// <summary>
        /// Please implement this method to provide the string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return ExpressionID;
        }

        /// <summary>
        /// Please implement this method to return a deep clone of the current object
        /// </summary>
        /// <returns></returns>
        public override ExpressionValue GetClone() {
            return new MessageLog(this);
        }

        /// <summary>
        /// Please implement this method to provide the compact string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ExpressionID {
            get {
                string returnString = String.Join("; ", (new List<ProposalMessage>(this.proposals.Values)).Select(proposal => proposal.ToString()).ToArray()) + "; ";
                foreach (KeyValuePair<int, List<PrevoteMessage>> entry in this.prevotes) {
                    returnString += String.Join("; ", entry.Value.Select(prevote => prevote.ToString()).ToArray()) + "; ";
                }
                foreach (KeyValuePair<int, List<PrecommitMessage>> entry in this.precommits) {
                    returnString += String.Join("; ", entry.Value.Select(precommit => precommit.ToString()).ToArray()) + "; ";
                }
                return returnString;
            }
        }
    }

    // public class MessageLogList : ExpressionValue {
    //     public List<MessageLog> messageLogs;

    //     public MessageLogList(int n, int f) {
    //         this.messageLogs = new List<MessageLog>();
    //         for (int i = 0; i < n; i++) {
    //             this.messageLogs.Add(new MessageLog(f));
    //         }
    //     }

    //     public MessageLogList(MessageLogList messageLogList) {
    //         this.messageLogs = new List<MessageLog>();
    //         foreach (MessageLog messageLog in messageLogList.messageLogs) {
    //             this.messageLogs.Add(new MessageLog(messageLog));
    //         }
    //     }

    //     public void AddProposal(int processId, ProposalMessage proposal) {
    //         this.messageLogs[processId].AddProposal(proposal);
    //     }

    //     public void AddPrevote(int processId, PrevoteMessage prevote) {
    //         this.messageLogs[processId].AddPrevote(prevote);
    //     }

    //     public void AddPrecommit(int processId, PrecommitMessage precommit) {
    //         this.messageLogs[processId].AddPrecommit(precommit);
    //     }

    //     public int GetProposalValue(int processId, int round) {
    //         return this.messageLogs[processId].GetProposalValue(round);
    //     }

    //     public int GetProposalValidRound(int processId, int round) {
    //         return this.messageLogs[processId].GetProposalValidRound(round);
    //     }

    //     public void Clear(int processId) {
    //         this.messageLogs[processId].Clear();
    //     }
        
    //     public bool ContainsProposal(int processId, int round, int validRound) {
    //         return this.messageLogs[processId].ContainsProposal(round, validRound);
    //     }

    //     public bool ContainsSufficientPrevotes(int processId, int round, int value) {
    //         return this.messageLogs[processId].ContainsSufficientPrevotes(round, value);
    //     }

    //     public bool ContainsSufficientPrecommits(int processId, int round, int value) {
    //         return this.messageLogs[processId].ContainsSufficientPrecommits(round, value);
    //     }

    //     public bool ContainsProposalAndSufficientPrevotesForPrevoting(int processId, int round) {
    //         return this.messageLogs[processId].ContainsProposalAndSufficientPrevotesForPrevoting(round);
    //     }

    //     public bool ContainsProposalAndSufficientPrevotesForPrecommitting(int processId, int round) {
    //         return this.messageLogs[processId].ContainsProposalAndSufficientPrevotesForPrecommitting(round);
    //     }

    //     public bool ContainsProposalAndSufficientPrecommits(int processId) {
    //         return this.messageLogs[processId].ContainsProposalAndSufficientPrecommits();
    //     }

    //     public bool ContainsProposalAndSufficientAdversePrecommits(int processId, int round) {
    //         return this.messageLogs[processId].ContainsProposalAndSufficientAdversePrecommits(round);
    //     }

    //     public int GetLatestRoundWithSufficientMessages(int processId) {
    //         return this.messageLogs[processId].GetLatestRoundWithSufficientMessages();
    //     }

    //     public int GetCommitReadyValue(int processId) {
    //         return this.messageLogs[processId].GetCommitReadyValue();
    //     }

    //     /// <summary>
    //     /// Please implement this method to provide the string representation of the datatype
    //     /// </summary>
    //     /// <returns></returns>
    //     public override string ToString() {
    //         return ExpressionID;
    //     }

    //     /// <summary>
    //     /// Please implement this method to return a deep clone of the current object
    //     /// </summary>
    //     /// <returns></returns>
    //     public override ExpressionValue GetClone() {
    //         return new MessageLogList(this);
    //     }

    //     /// <summary>
    //     /// Please implement this method to provide the compact string representation of the datatype
    //     /// </summary>
    //     /// <returns></returns>
    //     public override string ExpressionID {
    //         get {
    //             string returnString = "|| ";
    //             foreach (MessageLog messageLog in this.messageLogs) {
    //                 returnString += messageLog.ToString() + " || ";
    //             }
    //             return returnString;
    //         }
    //     }
    // }
}
