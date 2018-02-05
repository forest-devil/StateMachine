using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateMachine;
using StateMachine.AutoMapper;
using StateMachine.Extensions;
using StateMachine.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMachineTest
{
    [TestClass]
    public class StateMachineUnitTest
    {
        private List<ArticleEntity> articles = new List<ArticleEntity>
        {
            new ArticleEntity{ Title = "文章1", Status = ArticleStatus.已修改.ToString()},
            new ArticleEntity{ Title = "文章2", Status = ArticleStatus.已提交.ToString()},
            new ArticleEntity{ Title = "文章3", Status = ArticleStatus.已发布.ToString()},
            new ArticleEntity{ Title = "文章4", Status = ArticleStatus.已提交.ToString()},
            new ArticleEntity{ Title = "文章5", Status = ArticleStatus.已发布.ToString()},
            new ArticleEntity{ Title = "文章6", Status = ArticleStatus.已发布.ToString()},
        };

        static StateMachineUnitTest()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringStatusJsonConverter() }
            };
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<StateMachineAutoMapperProfile>();
            });
        }

        [TestMethod]
        public void Test01_ConstructorAndToString()
        {
            //这种写法用于在代码里手写，防止打错字
            var s1 = new MyStatus(ArticleStatus.已提交);
            Assert.AreEqual(ArticleStatus.已提交, s1.Value);
            Assert.AreEqual("已提交", s1.ToString());
            Assert.IsNotNull(s1.Workflow);
            Assert.IsNotNull(s1.Workflow.ValidStatuses);

            //这种方法适用于字段类型为string的Entity，省去了Parse
            var article2 = new ArticleEntity { Status = "已发布" };
            var s2 = new MyStatus(article2.Status);
            Assert.AreEqual(ArticleStatus.已发布, s2.Value);
            Assert.AreEqual("已发布", s2.ToString());

            //也可以接受字段类型为enum的Entity
            var article3 = new ArticleEntityOfEnum { Status = ArticleStatus.已发布 };
            var s3 = new MyStatus(article3.Status);
            Assert.AreEqual(ArticleStatus.已发布, s3.Value);
            Assert.AreEqual("已发布", s3.ToString());

            //试图用不合法的状态进行初始化，会默认设置为合法的第一个状态
            var s31 = new MyStatus(ArticleStatus.无效状态);
            Assert.AreEqual(ArticleStatus.已修改, s31.Value);

            var s32 = new MyStatus(Singleton<MySimplifiedWorkflow>.Instance, ArticleStatus.已存档);
            Assert.IsInstanceOfType(s32.Workflow, typeof(MySimplifiedWorkflow));
            Assert.AreEqual(ArticleStatus.已修改, s32.Value);

            //序列化、反序列化
            var s4 = new MyStatus(ArticleStatus.已发布);
            Assert.AreEqual("\"已发布\"", JsonConvert.SerializeObject(s4));
            var d4 = JsonConvert.DeserializeObject<MyStatus>("\"已发布\"");
            Assert.AreEqual(ArticleStatus.已发布, d4.Value);

            var s5 = MyComplicatedStatus.CreateInstance(ArticleStatus.已发布);
            Assert.AreEqual("\"已发布\"", JsonConvert.SerializeObject(s4));
            var d5 = JsonConvert.DeserializeObject<MyComplicatedStatus>("\"已发布\"");
            Assert.AreEqual(ArticleStatus.已发布, d5.Value);

            var domainArticle = Mapper.Map<DomainArticle>(articles[5]);
            Assert.AreEqual("{\"Content\":null,\"Status\":\"已发布\",\"Title\":\"文章6\"}", JsonConvert.SerializeObject(domainArticle));
            var dto = (DomainArticle)JsonConvert.DeserializeObject("{\"Content\":null,\"Status\":\"已发布\",\"Title\":\"文章6\"}", typeof(DomainArticle));
            Assert.AreEqual(ArticleStatus.已发布, dto.Status.Value);
        }

        [TestMethod]
        public void Test02_TransitionTest()
        {
            var myStatus = new MyStatus(ArticleStatus.已修改);
            Assert.AreEqual(ArticleStatus.已修改, myStatus.Value);

            // 试图进行不支持的操作，不引起状态变化
            myStatus.Transition(ArticleOperation.发布);
            Assert.AreEqual(ArticleStatus.已修改, myStatus.Value);

            // 以下为正确的流程
            myStatus.Transition(ArticleOperation.提交);
            Assert.AreEqual(ArticleStatus.已提交, myStatus.Value);
            myStatus.Transition(ArticleOperation.发布);
            Assert.AreEqual(ArticleStatus.已发布, myStatus.Value);
            myStatus.Transition(ArticleOperation.撤回);
            Assert.AreEqual(ArticleStatus.已修改, myStatus.Value);
        }

        [TestMethod]
        public void Test03_AddOperatorTest()
        {
            var s1 = new MyStatus(ArticleStatus.已修改);
            Assert.AreEqual(ArticleStatus.已修改, s1.Value);

            s1 += ArticleOperation.提交;
            Assert.AreEqual(ArticleStatus.已提交, s1.Value);

            s1 += ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, s1.Value);

            s1 += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, s1.Value);

            //快进模式
            var s2 = new MyStatus(ArticleStatus.已修改);
            Assert.AreEqual(ArticleStatus.已修改, s1.Value);
            s2 = s2 + ArticleOperation.提交 + ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, s2.Value);

            //忽略错误的操作（以下第1步中已修改状态不能进行撤回操作，故状态不变）
            var s3 = new MyStatus(ArticleStatus.已修改);
            s3 = s3 + ArticleOperation.撤回 + ArticleOperation.提交 + ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, s2.Value);

            //使用不同的工作流
            var s4 = new MyStatus(Singleton<MySimplifiedWorkflow>.Instance, ArticleStatus.已修改);
            Assert.AreEqual(ArticleStatus.已修改, s4.Value);
            s4 += ArticleOperation.提交;      // 这个工作流不支持提交操作，状态应当不变
            Assert.AreEqual(ArticleStatus.已修改, s4.Value);
            s4 += ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, s4.Value);
            s4 += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, s4.Value);

            //使用不同的工作流
            MyComplicatedStatus s5 = Singleton<已修改>.Instance;
            Assert.AreEqual(ArticleStatus.已修改, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已修改));
            s5.Edit(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddRemark(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddComment(null));
            s5 += ArticleOperation.发布;      // 这个工作流允许直接从已修改状态发布
            Assert.AreEqual(ArticleStatus.已发布, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已发布));
            s5.AddComment(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.Edit(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddRemark(null));
            s5 += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已修改));
            s5.Edit(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddRemark(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddComment(null));
            s5 += ArticleOperation.提交;
            Assert.AreEqual(ArticleStatus.已提交, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已提交));
            s5.AddRemark(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.Edit(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddComment(null));
            s5 += ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已发布));
            s5.AddComment(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.Edit(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddRemark(null));
            s5 += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, s5.Value);
            Assert.IsInstanceOfType(s5, typeof(已修改));
            s5.Edit(null);
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddRemark(null));
            Assert.ThrowsException<InvalidOperationException>(() => s5.AddComment(null));
        }

        [TestMethod]
        public void Test04_UsedStatusesAndOperations()
        {
            var w1 = Singleton<MyWorkflow>.Instance;
            Assert.AreEqual("已修改,已提交,已发布,已存档", string.Join(",", w1.ValidStatuses));
            Assert.AreEqual("提交,发布,撤回,存档", string.Join(",", w1.ValidOperations));

            var w2 = Singleton<MySimplifiedWorkflow>.Instance;
            Assert.AreEqual("已修改,已发布", string.Join(",", w2.ValidStatuses));
            Assert.AreEqual("发布,撤回", string.Join(",", w2.ValidOperations));

            var w3 = Singleton<MyComplicatedWorkflow>.Instance;
            Assert.AreEqual("已修改,已提交,已发布", string.Join(",", w3.ValidStatuses));
            Assert.AreEqual("提交,发布,撤回", string.Join(",", w3.ValidOperations));
        }

        [TestMethod]
        public void Test05_01_FilterByStatus()
        {
            //模拟从数据库取出Entity，类型是IQueriable<T>
            var queriedArticles = articles.AsQueryable();

            var q1 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已修改);
            Assert.AreEqual("文章1", string.Join(",", q1.Select(a => a.Title)));

            var q2 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交);
            Assert.AreEqual("文章2,文章4", string.Join(",", q2.Select(a => a.Title)));

            var q3 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已发布);
            Assert.AreEqual("文章3,文章5,文章6", string.Join(",", q3.Select(a => a.Title)));

            var q4 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交, ArticleStatus.已发布);
            Assert.AreEqual("文章2,文章3,文章4,文章5,文章6", string.Join(",", q4.Select(a => a.Title)));
        }

        [TestMethod]
        public void Test05_02_FilterByStatusForEntitiesWithEnumProperty()
        {
            // 模拟从数据库取出Entity，类型是IQueriable<T>。
            // 这种Entity的Status字段用了Enum类型，为了方便此处直接映射转换
            var queriedArticles = Mapper.Map<IEnumerable<ArticleEntityOfEnum>>(articles).AsQueryable();

            var q1 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已修改);
            Assert.AreEqual("文章1", string.Join(",", q1.Select(a => a.Title)));

            var q2 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交);
            Assert.AreEqual("文章2,文章4", string.Join(",", q2.Select(a => a.Title)));

            var q3 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已发布);
            Assert.AreEqual("文章3,文章5,文章6", string.Join(",", q3.Select(a => a.Title)));

            var q4 = queriedArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交, ArticleStatus.已发布);
            Assert.AreEqual("文章2,文章3,文章4,文章5,文章6", string.Join(",", q4.Select(a => a.Title)));
        }

        [TestMethod]
        public void Test05_03_FilterByStatusForDto()
        {
            //模拟从数据库取出并映射成Dto，类型是IEnumerable<T>
            var domainArticles = Mapper.Map<IEnumerable<DomainArticle>>(articles);

            var q1 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已修改);
            Assert.AreEqual("文章1", string.Join(",", q1.Select(a => a.Title)));

            var q2 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交);
            Assert.AreEqual("文章2,文章4", string.Join(",", q2.Select(a => a.Title)));

            var q3 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已发布);
            Assert.AreEqual("文章3,文章5,文章6", string.Join(",", q3.Select(a => a.Title)));

            var q4 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交, ArticleStatus.已发布);
            Assert.AreEqual("文章2,文章3,文章4,文章5,文章6", string.Join(",", q4.Select(a => a.Title)));
        }

        [TestMethod]
        public void Test06_AutoMap()
        {
            Assert.AreEqual(ArticleStatus.已发布, (Mapper.Map<MyStatus>("已发布")).Value);
            Assert.AreEqual(ArticleStatus.已发布, (Mapper.Map<MyStatus>(ArticleStatus.已发布)).Value);

            var sts = new MyStatus(ArticleStatus.已发布);
            Assert.AreEqual("已发布", Mapper.Map<string>(sts));
            Assert.AreEqual(ArticleStatus.已发布, Mapper.Map<ArticleStatus>(sts));

            //从Entity映射到Dto
            var domainArticle = Mapper.Map<DomainArticle>(articles[5]);
            Assert.AreEqual("已发布", domainArticle.Status.ToString());

            //映射
            var s1 = new MyStatus(ArticleStatus.已发布);
            Assert.AreEqual("已发布", Mapper.Map<string>(s1));

            //模拟从Entity映射到Dto
            domainArticle = Mapper.Map<DomainArticle>(articles[0]);
            Assert.AreEqual("已修改", domainArticle.Status.ToString());

            domainArticle.Status += ArticleOperation.提交;
            Assert.AreEqual(ArticleStatus.已提交, domainArticle.Status.Value);

            domainArticle.Status += ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, domainArticle.Status.Value);

            domainArticle.Status += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, domainArticle.Status.Value);

            //模拟从Dto映射到Entity
            domainArticle.Status += ArticleOperation.提交;
            var articleEntity = Mapper.Map<ArticleEntity>(domainArticle);
            Assert.AreEqual("已提交", articleEntity.Status);

            //模拟从数据库取出并映射成Dto，类型是IEnumerable<T>
            var domainArticles = Mapper.Map<IEnumerable<DomainArticle>>(articles);

            var q1 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已修改);
            Assert.AreEqual("文章1", string.Join(",", q1.Select(a => a.Title)));

            var q2 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交);
            Assert.AreEqual("文章2,文章4", string.Join(",", q2.Select(a => a.Title)));

            var q3 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已发布);
            Assert.AreEqual("文章3,文章5,文章6", string.Join(",", q3.Select(a => a.Title)));

            var q4 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交, ArticleStatus.已发布);
            Assert.AreEqual("文章2,文章3,文章4,文章5,文章6", string.Join(",", q4.Select(a => a.Title)));
        }

        [TestMethod]
        public void Test07_AutoMapForEntityOfEnum()
        {
            //模拟从Entity映射到Dto
            var entity = new ArticleEntityOfEnum { Title = "另一种文章1", Status = ArticleStatus.已发布 };
            var domainArticle = Mapper.Map<DomainArticle>(entity);
            Assert.AreEqual(ArticleStatus.已发布.ToString(), domainArticle.Status.ToString());

            domainArticle.Status += ArticleOperation.撤回;
            Assert.AreEqual(ArticleStatus.已修改, domainArticle.Status.Value);

            domainArticle.Status += ArticleOperation.提交;
            Assert.AreEqual(ArticleStatus.已提交, domainArticle.Status.Value);

            domainArticle.Status += ArticleOperation.发布;
            Assert.AreEqual(ArticleStatus.已发布, domainArticle.Status.Value);

            //模拟从Dto映射到Entity
            var articleEntity = Mapper.Map<ArticleEntityOfEnum>(domainArticle);
            Assert.AreEqual(ArticleStatus.已发布, articleEntity.Status);

            //模拟从数据库取出
            var entities = Mapper.Map<IEnumerable<ArticleEntityOfEnum>>(articles).AsQueryable();

            var f1 = entities.FilterByStatus(article => article.Status, ArticleStatus.已修改);
            Assert.AreEqual("文章1", string.Join(",", f1.Select(a => a.Title)));

            //映射成Dto
            var domainArticles = Mapper.Map<IEnumerable<DomainArticle>>(entities);

            var d2 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交);
            Assert.AreEqual("文章2,文章4", string.Join(",", d2.Select(a => a.Title)));

            var d3 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已发布);
            Assert.AreEqual("文章3,文章5,文章6", string.Join(",", d3.Select(a => a.Title)));

            var d4 = domainArticles.FilterByStatus(article => article.Status, ArticleStatus.已提交, ArticleStatus.已发布);
            Assert.AreEqual("文章2,文章3,文章4,文章5,文章6", string.Join(",", d4.Select(a => a.Title)));
        }
    }
}